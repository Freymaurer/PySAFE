namespace Components

open Feliz
open Feliz.DaisyUI
open Fable.Remoting.Client

module private Helper =

    [<Literal>]
    let SingleSequence = """MSTAPVKKPVNLPFLPKDVEDLVLWRKPKEAGAIFGGATAAYLAYVYNPFNGFTIVSYLLSIISLALFLWSHLGHFVSRSGPPVPEFLVKGVTQEQARQVADAALPVVNKALGYVGVLASGKDLKTSSLVVVGSYTAGRIFALASPFTLAYVVVVLAFVLPKAYEAKQDEVDKVLAVVKAKVDEAVTAFNNNVLSKIPKAQPPAPKKVD*"""

    [<Literal>]
    let MultipleSequences = """>!MSGAETHRLALLASSRALIARVQLGDAPAALSQALSTLSSVIYASTSYGRSECSCSSSSGREVQPSATPASPATAQTSRVAPFLALQRPSAAFSRLGLSTWAPHASVSGWHGARQLHAGSAAQQAADVPGGSASGSGGKESEGAAKPQEQTVTNPLAAALASAASGPVASGSASAAGGLAQAAQAAAGRGRRQPRNWMWYDADDEREERRKERQRLAWAMGPGGGEEVGAAHLMDVPQSMKKMQRIVKLVRGLPYPDAVAQCSLVPHKAARYMLQALEAAHADATEVKGLDAERLVVGTVFVTRGAYEPGISYHSKGRPGSKTFHRSHIRVVLNQAAERPAPFARIVAPLMSRRGLLGGSGGAGGAPRPRFAYRTEV*
>!MSTAPVKKPVNLPFLPKDVEDLVLWRKPKEAGAIFGGATAAYLAYVYNPFNGFTIVSYLLSIISLALFLWSHLGHFVSRSGPPVPEFLVKGVTQEQARQVADAALPVVNKALGYVGVLASGKDLKTSSLVVVGSYTAGRIFALASPFTLAYVVVVLAFVLPKAYEAKQDEVDKVLAVVKAKVDEAVTAFNNNVLSKIPKAQPPAPKKVD*"""

    [<Literal>]
    let NamedFasta = """>Cre03.g207377
MQSLRSAASSRSCPYMPRTGRQLVPTPFRVSVSAGRSRAPVATQPARDVHLAGSMATSSGPAQDWRGEVPQSPLPSVAPVSPAKAVKWFFPSAAQPLASVEEAFSEVVAITTDATRLLPATLVSIFAAAWPVIAKAWTQPVGDTVRILTAGTCALVSRLSELAAAAATILAAGAAAAAATTTTTTTSRLAATSATAQTATATSTPPSAVYDTAAANKYLEGTAFARHAVLATSCQQVLELTRVCTCAGKPFAIALMPDYTYFDKSAQLDALAEMASASGSLLIACSIAPGFYKRAAAAADMMGTPEGWDASAADVERQFGLDVALASRIPGARNRGILEPVTGMPAFMFGGWFVAHNGPLSNYACTSGCELPLSSPFKLLDAMGIFKRLLEWRQKQAAVARLAVGNH*
>Cre05.g242950
MSGAETHRLALLASSRALIARVQLGDAPAALSQALSTLSSVIYASTSYGRSECSCSSSSGREVQPSATPASPATAQTSRVAPFLALQRPSAAFSRLGLSTWAPHASVSGWHGARQLHAGSAAQQAADVPGGSASGSGGKESEGAAKPQEQTVTNPLAAALASAASGPVASGSASAAGGLAQAAQAAAGRGRRQPRNWMWYDADDEREERRKERQRLAWAMGPGGGEEVGAAHLMDVPQSMKKMQRIVKLVRGLPYPDAVAQCSLVPHKAARYMLQALEAAHADATEVKGLDAERLVVGTVFVTRGAYEPGISYHSKGRPGSKTFHRSHIRVVLNQAAERPAPFARIVAPLMSRRGLLGGSGGAGGAPRPRFAYRTEV*
>Cre06.g308950
MSTAPVKKPVNLPFLPKDVEDLVLWRKPKEAGAIFGGATAAYLAYVYNPFNGFTIVSYLLSIISLALFLWSHLGHFVSRSGPPVPEFLVKGVTQEQARQVADAALPVVNKALGYVGVLASGKDLKTSSLVVVGSYTAGRIFALASPFTLAYVVVVLAFVLPKAYEAKQDEVDKVLAVVKAKVDEAVTAFNNNVLSKIPKAQPPAPKKVD*
"""

    let FastaFile = [| 62uy; 115uy; 112uy; 124uy; 65uy; 48uy; 65uy; 49uy; 55uy; 56uy; 86uy; 69uy; 75uy; 55uy; 124uy; 68uy; 85uy; 79uy; 49uy; 95uy; 65uy; 82uy; 65uy; 84uy; 72uy; 32uy; 84uy; 114uy; 97uy; 110uy; 115uy; 99uy; 114uy; 105uy; 112uy; 116uy; 105uy; 111uy; 110uy; 32uy; 102uy; 97uy; 99uy; 116uy; 111uy; 114uy; 32uy; 68uy; 85uy; 79uy; 49uy; 32uy; 79uy; 83uy; 61uy; 65uy; 114uy; 97uy; 98uy; 105uy; 100uy; 111uy; 112uy; 115uy; 105uy; 115uy; 32uy; 116uy; 104uy; 97uy; 108uy; 105uy; 97uy; 110uy; 97uy; 32uy; 79uy; 88uy; 61uy; 51uy; 55uy; 48uy; 50uy; 32uy; 71uy; 78uy; 61uy; 68uy; 85uy; 79uy; 49uy; 32uy; 80uy; 69uy; 61uy; 49uy; 32uy; 83uy; 86uy; 61uy; 49uy; 13uy; 10uy; 77uy; 82uy; 75uy; 77uy; 69uy; 65uy; 75uy; 75uy; 69uy; 69uy; 73uy; 75uy; 75uy; 71uy; 80uy; 87uy; 75uy; 65uy; 69uy; 69uy; 68uy; 69uy; 86uy; 76uy; 73uy; 78uy; 72uy; 86uy; 75uy; 82uy; 89uy; 71uy; 80uy; 82uy; 68uy; 87uy; 83uy; 83uy; 73uy; 82uy; 83uy; 75uy; 71uy; 76uy; 76uy; 81uy; 82uy; 84uy; 71uy; 75uy; 83uy; 67uy; 82uy; 76uy; 82uy; 87uy; 86uy; 78uy; 75uy; 76uy; 13uy; 10uy; 82uy; 80uy; 78uy; 76uy; 75uy; 78uy; 71uy; 67uy; 75uy; 70uy; 83uy; 65uy; 68uy; 69uy; 69uy; 82uy; 84uy; 86uy; 73uy; 69uy; 76uy; 81uy; 83uy; 69uy; 70uy; 71uy; 78uy; 75uy; 87uy; 65uy; 82uy; 73uy; 65uy; 84uy; 89uy; 76uy; 80uy; 71uy; 82uy; 84uy; 68uy; 78uy; 68uy; 86uy; 75uy; 78uy; 70uy; 87uy; 83uy; 83uy; 82uy; 81uy; 75uy; 82uy; 76uy; 65uy; 82uy; 73uy; 76uy; 72uy; 13uy; 10uy; 78uy; 83uy; 83uy; 68uy; 65uy; 83uy; 83uy; 83uy; 83uy; 70uy; 78uy; 80uy; 75uy; 83uy; 83uy; 83uy; 83uy; 72uy; 82uy; 76uy; 75uy; 71uy; 75uy; 78uy; 86uy; 75uy; 80uy; 73uy; 82uy; 81uy; 83uy; 83uy; 81uy; 71uy; 70uy; 71uy; 76uy; 86uy; 69uy; 69uy; 69uy; 86uy; 84uy; 86uy; 83uy; 83uy; 83uy; 67uy; 83uy; 81uy; 77uy; 86uy; 80uy; 89uy; 83uy; 83uy; 68uy; 81uy; 86uy; 71uy; 13uy; 10uy; 68uy; 69uy; 86uy; 76uy; 82uy; 76uy; 80uy; 68uy; 76uy; 71uy; 86uy; 75uy; 76uy; 69uy; 72uy; 81uy; 80uy; 70uy; 65uy; 70uy; 71uy; 84uy; 68uy; 76uy; 86uy; 76uy; 65uy; 69uy; 89uy; 83uy; 68uy; 83uy; 81uy; 78uy; 68uy; 65uy; 78uy; 81uy; 81uy; 65uy; 73uy; 83uy; 80uy; 70uy; 83uy; 80uy; 69uy; 83uy; 82uy; 69uy; 76uy; 76uy; 65uy; 82uy; 76uy; 68uy; 68uy; 80uy; 70uy; 89uy; 13uy; 10uy; 89uy; 68uy; 73uy; 76uy; 71uy; 80uy; 65uy; 68uy; 83uy; 83uy; 69uy; 80uy; 76uy; 70uy; 65uy; 76uy; 80uy; 81uy; 80uy; 70uy; 70uy; 69uy; 80uy; 83uy; 80uy; 86uy; 80uy; 82uy; 82uy; 67uy; 82uy; 72uy; 86uy; 83uy; 75uy; 68uy; 69uy; 69uy; 65uy; 68uy; 86uy; 70uy; 76uy; 68uy; 68uy; 70uy; 80uy; 65uy; 68uy; 77uy; 70uy; 68uy; 81uy; 86uy; 68uy; 80uy; 73uy; 80uy; 83uy; 80uy; 13uy; 10uy; 62uy; 115uy; 112uy; 124uy; 65uy; 48uy; 65uy; 49uy; 55uy; 56uy; 87uy; 70uy; 53uy; 54uy; 124uy; 67uy; 83uy; 84uy; 77uy; 51uy; 95uy; 65uy; 82uy; 65uy; 84uy; 72uy; 32uy; 80uy; 114uy; 111uy; 116uy; 101uy; 105uy; 110uy; 32uy; 67uy; 89uy; 83uy; 84uy; 69uy; 73uy; 78uy; 69uy; 45uy; 82uy; 73uy; 67uy; 72uy; 32uy; 84uy; 82uy; 65uy; 78uy; 83uy; 77uy; 69uy; 77uy; 66uy; 82uy; 65uy; 78uy; 69uy; 32uy; 77uy; 79uy; 68uy; 85uy; 76uy; 69uy; 32uy; 51uy; 32uy; 79uy; 83uy; 61uy; 65uy; 114uy; 97uy; 98uy; 105uy; 100uy; 111uy; 112uy; 115uy; 105uy; 115uy; 32uy; 116uy; 104uy; 97uy; 108uy; 105uy; 97uy; 110uy; 97uy; 32uy; 79uy; 88uy; 61uy; 51uy; 55uy; 48uy; 50uy; 32uy; 71uy; 78uy; 61uy; 67uy; 89uy; 83uy; 84uy; 77uy; 51uy; 32uy; 80uy; 69uy; 61uy; 49uy; 32uy; 83uy; 86uy; 61uy; 49uy; 13uy; 10uy; 77uy; 65uy; 81uy; 89uy; 72uy; 81uy; 81uy; 72uy; 69uy; 77uy; 75uy; 81uy; 84uy; 77uy; 65uy; 69uy; 84uy; 81uy; 89uy; 86uy; 84uy; 65uy; 80uy; 80uy; 80uy; 77uy; 71uy; 89uy; 80uy; 86uy; 77uy; 77uy; 75uy; 68uy; 83uy; 80uy; 81uy; 84uy; 86uy; 81uy; 80uy; 80uy; 72uy; 69uy; 71uy; 81uy; 83uy; 75uy; 71uy; 83uy; 71uy; 71uy; 70uy; 76uy; 82uy; 71uy; 67uy; 76uy; 65uy; 65uy; 13uy; 10uy; 77uy; 67uy; 67uy; 67uy; 67uy; 86uy; 76uy; 68uy; 67uy; 86uy; 70uy |]

type Examples =

    [<ReactComponent>]
    static member Main (setData: string -> unit) =
        let ref = React.useElementRef()
        let close() =
            if ref.current.IsSome then
                ref.current.Value.blur()
        Daisy.dropdown [
            dropdown.top
            prop.style [style.textAlign.left]
            prop.children [
                Html.div [
                    prop.className "btn btn-info"
                    prop.role "button"
                    prop.tabIndex 0
                    prop.text "Examples"
                ]
                Daisy.dropdownContent [
                    prop.ref ref
                    prop.className "z-[1] menu p-2 shadow bg-base-100 rounded-box w-52"
                    prop.tabIndex 0
                    prop.children [
                        Html.li [Html.a [
                            prop.onClick(fun _ ->
                                Helper.SingleSequence |> setData
                                close()
                            )
                            prop.children [
                                Html.span "Single Sequence"
                                Html.i [
                                    prop.style [style.float'.right]
                                    prop.className [fa.faSolid; fa.faPen; "ml-auto"]
                                ]
                            ]
                        ]]
                        Html.li [Html.a [
                            prop.onClick(fun _ ->
                                Helper.MultipleSequences |> setData
                                close()
                            )
                            prop.children [
                                Html.span "Multiple Sequences"
                                Html.i [
                                    prop.style [style.float'.right]
                                    prop.className [fa.faSolid; fa.faPen; "ml-auto"]
                                ]
                            ]
                        ]]
                        Html.li [Html.a [
                            prop.onClick(fun _ ->
                                Helper.NamedFasta |> setData
                                close()
                            )
                            prop.children [
                                Html.span "Named Fasta"
                                Html.i [
                                    prop.style [style.float'.right]
                                    prop.className [fa.faSolid; fa.faPen; "ml-auto"]
                                ]
                            ]
                        ]]
                        Html.li [Html.a [
                            prop.onClick(fun _ ->
                                Helper.FastaFile.SaveFileAs("Example.fasta")
                                close()
                            )
                            prop.children [
                                Html.span "Fasta File"
                                Html.i [
                                    prop.style [style.float'.right]
                                    prop.className [fa.faSolid; fa.faDownload; "ml-auto"]
                                ]
                            ]
                        ]]
                        Daisy.divider [
                        ]
                        Html.li [Html.a [
                            //prop.href Shared.Urls.GitHubFormatInfo //TODO
                            prop.target "_blank"
                            prop.children [
                                Daisy.labelTextAlt [
                                    color.textInfo
                                    prop.text "Click here for more information"
                                ]
                            ]
                        ]]
                    ]
                ]
            ]
        ]