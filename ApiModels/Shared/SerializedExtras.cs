using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ApiModels.Shared
{
    //the intent of this class is to allow for extra options that is not part of the base class
    //this way a plugin can add extra options to the base class without having to modify the base class
    public class SerializedExtras
    {
        public string? ItemName { get; set; } //this is the name of the extra option
        public byte[]? ItemValue { get; set; } //this is the value of the extra option after being serialized
        public SerialzedExtraClientUIType ClientUIType { get; set; } = SerialzedExtraClientUIType.None; //this is the type of UI element to use when displaying the extra option
        public int? ItemId { get; set; } //Optional field that can be set by the plugin to help identify the extra option, can be useful when the plugin has multiple extra options with the same name

    }

    public enum SerialzedExtraClientUIType
    {
        None = 0,
        CheckBox = 1,
        ColorPicker = 2,
        DatePicker = 3,
        DateTimePicker = 4,
        NumericField = 5,
        SingleSelect = 6,
        MultiSelect = 7,
        TextField = 8,
        ToggleSwitch = 9,
    }
}
