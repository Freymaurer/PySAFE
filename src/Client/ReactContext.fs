module ReactContext

open Fable.React
open Feliz

let modalContext: IContext<IRefValue<Browser.Types.HTMLElement option>> = React.createContext(name="Modal")

