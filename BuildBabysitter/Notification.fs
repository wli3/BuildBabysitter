namespace BuildBabysitter

type public INotification =
    abstract member Show: string -> string -> unit
