namespace HardHatCore.ApiModels.Plugin_Interfaces
{
    //this structure should allow for overrides that can add new types of responses as needed
    //public class ExtImplantTaskResponseType
    //{
    //    public string Value { get; private set; }

    //    public ExtImplantTaskResponseType(string value)
    //    {
    //        Value = value;
    //    }

    //    public static readonly ExtImplantTaskResponseType None = new ExtImplantTaskResponseType("None");
    //    public static readonly ExtImplantTaskResponseType String = new ExtImplantTaskResponseType("String");
    //    public static readonly ExtImplantTaskResponseType FileSystemItem = new ExtImplantTaskResponseType("FileSystemItem");
    //    public static readonly ExtImplantTaskResponseType ProcessItem = new ExtImplantTaskResponseType("ProcessItem");
    //    public static readonly ExtImplantTaskResponseType HelpMenuItem = new ExtImplantTaskResponseType("HelpMenuItem");
    //    public static readonly ExtImplantTaskResponseType TokenStoreItem = new ExtImplantTaskResponseType("TokenStoreItem");
    //    public static readonly ExtImplantTaskResponseType DataChunk = new ExtImplantTaskResponseType("DataChunk");
    //}

    public enum ExtImplantTaskResponseType
    {
        None,
        String,
        FileSystemItem,
        ProcessItem,
        HelpMenuItem,
        TokenStoreItem,
        DataChunk,
        EditFile,
    }
}
