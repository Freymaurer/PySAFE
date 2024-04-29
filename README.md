# SAFE Template

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* [.NET SDK](https://www.microsoft.com/net/download) 8.0 or higher
* [Node 18](https://nodejs.org/en/download/) or higher
* [NPM 9](https://www.npmjs.com/package/npm) or higher

## Starting the application

## Request Workflow

```mermaid
sequenceDiagram
    participant py as Python ML
    participant net as F#35; Server
    participant c as Client
    actor u as User
    u -->> c: Gives data
    c -->>+net: sends user data
    par start analysis
    net-)+py: sends data, trigger eval
    py-)net: returns binned data
    and return request information
    net -) c: returns `request-ID`
    end
    critical ⚠️
    u -->> c: copies and stores `request-ID`
    end
    opt email
    u -->> c: give email address
    c -->> net: give id + email to store
    end
    opt check status
    u -->> c: use `request-ID` to check status
    end
    py-)net: send last package
    deactivate py
    net-->>net: run q-value calculation
    net-->>net: store results
    deactivate net
    opt gave email
    net-)u: send email
    end
    u -->> c: request data
    c-->>net: get data
    net-->>c: return data
    c-->>u: download data
```
