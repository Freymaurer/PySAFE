from regex import B
import torch
from pathlib import Path
from torch.utils.data import Dataset, DataLoader
import numpy as np
from torch import nn
import torch.nn.functional as F
from torch.nn.utils.rnn import pad_sequence
import json

def collate_fn (batch):
    pad_value = 0.
    features = pad_sequence(batch, batch_first=True, padding_value=pad_value)
    mask = ~(features == pad_value)
    mask = mask.float()
    mask = mask.mean(dim=-1)
    mask = mask > 0.1
    return features, mask

def generate_embedding (fasta, tokenizer, model):
    protein_seq = [list(fasta)]
    outputs = tokenizer.batch_encode_plus(protein_seq,
                                          add_special_tokens=True,
                                          padding=True,
                                          is_split_into_words=True,
                                          return_tensors='pt')
    with torch.no_grad():
        embeddings = model(input_ids=outputs['input_ids'])[0]
    emb = embeddings[0].detach().to('cpu').numpy()[:-1]
    emb = torch.from_numpy(emb)
    emb = emb.unsqueeze(0)
    return emb
    
class attention_pooling (nn.Module):
    def __init__(self, input_dim, dropout):
        super().__init__()
        self.query = nn.Linear(input_dim, input_dim)
        self.key = nn.Linear(input_dim, input_dim)
        self.value = nn.Linear(input_dim, input_dim)
        self.dropout = nn.Dropout(dropout)

    def forward(self, x):
        query = self.query(x[:,0]).unsqueeze(1)
        key = self.key(x)
        value = self.value(x)
        attention = torch.matmul(query, key.transpose(-2,-1))
        attention = attention / torch.sqrt(x.size(2))
        attention = F.softmax(attention, dim=-1)
        value = torch.matmul(attention, value)
        return value
    
class multihead_attention_pooling (nn.Module):
    def __init__(self, input_dim, num_heads, dropout):
        super().__init__()
        self.num_heads = num_heads
        self.query = nn.Linear(input_dim, input_dim)
        self.key = nn.Linear(input_dim, input_dim)
        self.value = nn.Linear(input_dim, input_dim)
        self.dropout = nn.Dropout(dropout/2)

    def forward(self, x, mask):
        B,L,E = x.size()
        query = self.query(x[:,0]).unsqueeze(1).reshape (B,1,self.num_heads,E//self.num_heads).permute(0,2,1,3)
        key = self.key(x).reshape(B,L,self.num_heads,E//self.num_heads).permute(0,2,1,3)
        value = self.value(x).reshape(B,L,self.num_heads,E//self.num_heads).permute(0,2,1,3)
        attention = torch.matmul(query, key.transpose(-2,-1))
        attention = attention / np.sqrt(E//self.num_heads)
        mask = mask.unsqueeze(1).unsqueeze(1).expand(-1,self.num_heads,1,L)
        attention = attention.masked_fill(mask == 0, -1e9)
        attention = F.softmax(attention, dim=-1)
        attention = self.dropout(attention)
        value = torch.matmul(attention, value)
        return value.reshape(B,1,E)
    
class feedforward_block (nn.Module):
    def __init__(self,emb_dim, dropout):
        super().__init__()
        self.norm = nn.LayerNorm(emb_dim)
        self.ff1 = nn.Linear(emb_dim, emb_dim)
        self.ff2 = nn.Linear(emb_dim, emb_dim)
        self.dropout = nn.Dropout(dropout)
    
    def forward(self,x):
        x = self.ff1(x)
        x = F.gelu(x)
        x = self.norm(x)
        x = self.dropout(x)
        x = self.ff2(x)
        x = self.dropout(x)
        return x

class LearnedAggregation (nn.Module):
    def __init__(self,input_dim,num_heads=1, dropout=0.0 ,init_value=1e-4) -> None:
        super().__init__()
        self.init_value = init_value
        self.num_heads = num_heads
        self.dropout = dropout
        self.normpre = nn.LayerNorm(input_dim)
        self.normpast = nn.LayerNorm(input_dim)
        self.normff = nn.LayerNorm(input_dim)
        self.attn = multihead_attention_pooling(input_dim, num_heads, dropout)
        self.gamma1 = nn.Parameter(self.init_value*torch.ones(input_dim), requires_grad=True)
        self.gamma2 = nn.Parameter(self.init_value*torch.ones(input_dim), requires_grad=True)
        self.ff = feedforward_block(input_dim, dropout)
    
    def forward(self, x, x_cls, mask):
        #u = torch.cat ((x, x_cls), dim=1)
        u = self.gamma1*self.normpast(self.attn(self.normpre(x),mask))
        x_cls = torch.add (x_cls, u)
        a = self.gamma2*self.normff(self.ff(x_cls))
        x_cls = torch.add (x_cls, a)
        return x_cls


class Loc_classifier (nn.Module):
    #initialisation of parameters and layers
    def __init__(self, dropout, label_size, emb_dim, n_blocks, batch_size, num_heads=1, init_value=1e-4):
        super().__init__()
        #parameters for the folliwing functions
        self.dropout =dropout
        
        #attetnion pooling
        self.cls = nn.Parameter (torch.zeros(1,int(emb_dim)), requires_grad=True)
        self.blocks = nn.ModuleList([LearnedAggregation(emb_dim, dropout=dropout, num_heads=num_heads, init_value=init_value) for _ in range(n_blocks)])

        #layer of the neureal network
        self.first_layer = nn.Linear (emb_dim, emb_dim)
        self.first_dropout = nn.Dropout1d (dropout)
        self.second_layer = nn.Linear (emb_dim, label_size)
        self.layer_norm1 = nn.LayerNorm (emb_dim)

    #forward pass through the neural network    
    def forward (self, x, mask):
        B,L,E = x.size()
        x_cls = self.cls.expand(B,1,-1)
        for block in self.blocks:
            x_cls = block (x, x_cls, mask)
        x_cls = x_cls.squeeze(1)
        x_cls = self.first_dropout (self.layer_norm1 (F.selu(self.first_layer(x_cls))))
        prediction = self.second_layer(x_cls)
        #prediction = F.sigmoid(x)
        return prediction

def prediction (fasta, tokenizer, model, predchloro, predmito, predsecreted):
    emb = [generate_embedding (seq.Sequence, tokenizer, model).view(-1,768) for seq in fasta]
    name_list = [seq.Header for seq in fasta]
    emb, mask = collate_fn(emb)
    with torch.no_grad():
        prediction_chloro = predchloro(emb,mask)
        prediction_chloro = torch.sigmoid(prediction_chloro)
        prediction_chloro = prediction_chloro[:,0]
        prediction_mito = predmito(emb,mask)
        prediction_mito = torch.sigmoid(prediction_mito)
        prediction_mito = prediction_mito[:,0]
        prediction_sp = predsecreted(emb,mask)
        prediction_sp = torch.sigmoid(prediction_sp)
        prediction_sp = prediction_sp[:,0]
        final_prediction = torch.stack((prediction_chloro, prediction_mito, prediction_sp), dim=1)
        final_prediction = final_prediction.tolist()
    json_data = json.dumps([
        {"Header": name,
         "Prediction":final_prediction[i]}
        for i,name in enumerate(name_list)
    ])

    return json_data