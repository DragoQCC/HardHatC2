using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HardHatCore.TeamServer.Utilities
{
    public class bof_pack
    {
        // Most code taken from: https://github.com/trustedsec/COFFLoader/blob/main/beacon_generate.py & https://gitlab.com/KevinJClark/badrats/-/blob/master/resources/bof_pack.py
        // Emulates the native Cobalt Strike bof_pack() function.
        // Documented here: https://hstechdocs.helpsystems.com/manuals/cobaltstrike/current/userguide/content/topics_aggressor-scripts/as-resources_functions.htm#bof_pack
        //
        // Type 	Description 				Unpack With (C)
        // --------|---------------------------------------|------------------------------
        // b       | binary data 			      |	BeaconDataExtract
        // i       | 4-byte integer 			      |	BeaconDataInt
        // s       | 2-byte short integer 		      |	BeaconDataShort
        // z       | zero-terminated+encoded string 	      |	BeaconDataExtract
        // Z       | zero-terminated wide-char string      |	(wchar_t *)BeaconDataExtract

        public static byte[] buffer = new byte[0];

        // create the functions for adding the different types of data to the buffer
        // add_binary
        public static bool add_binary(byte[] data)
        {
            try
            {
                // add the data to the buffer
                buffer = buffer.Concat(data).ToArray();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding binary data to buffer: " + ex.Message);
                return false;
            }

        }
        // add_int
        public static bool add_int(int data)
        {
            try
            {
                // convert the int to a byte array
                byte[] data_bytes = BitConverter.GetBytes(data);
                // add the data to the buffer
                buffer = buffer.Concat(data_bytes).ToArray();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding int data to buffer: " + ex.Message);
                return false;
            }
        }
        // add_short
        public static bool add_short(short data)
        {
            try
            {
                // Convert the short to a byte array
                byte[] data_bytes = BitConverter.GetBytes(data);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(data_bytes);
                }

                // Add the data to the buffer
                buffer = buffer.Concat(data_bytes).ToArray();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding short data to buffer: " + ex.Message);
                return false;
            }
        }
        // add_string
        public static bool add_string(string data)
        {
            try
            {
                // convert the string to a byte array
                byte[] data_bytes = System.Text.Encoding.ASCII.GetBytes(data);
                // add the data to the buffer
                buffer = buffer.Concat(data_bytes).ToArray();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding string data to buffer: " + ex.Message);
                return false;
            }
        }
        // add_wide_string
        public static bool add_wide_string(string data)
        {
            try
            {
                // Convert the string to a byte array
                byte[] data_bytes = System.Text.Encoding.Unicode.GetBytes(data);

                // Add null termination (two null bytes)
                data_bytes = data_bytes.Concat(new byte[] { 0, 0 }).ToArray();

                // Get the length of the string data (in bytes)
                int length = data_bytes.Length;
                byte[] length_bytes = BitConverter.GetBytes(length);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(length_bytes);
                }

                // Add the length and the string data to the buffer
                buffer = buffer.Concat(length_bytes).Concat(data_bytes).ToArray();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding wide string data to buffer: " + ex.Message);
                return false;
            }
        }


        public static bool Bof_Pack(string argTypes, List<object> argvalues, out string argHex)
        {
            try
            {
                //argTypes is a string like "bss" where each character represents the type of the corresponding argvalue 
                for (int i = 0; i < argTypes.Length; i++)
                {
                    string c = argTypes.Select(c => c.ToString()).ToList()[i];
                    string c_cleaned = c.Trim();
                    object argvalue = argvalues[i];
                    if (c_cleaned.Equals("b", StringComparison.CurrentCultureIgnoreCase))
                    {
                        byte[] argvalue_bytes = (byte[])argvalue;
                        bool result = add_binary(argvalue_bytes);
                        if (result == false)
                        {
                            argHex = "";
                            return false;
                        }

                    }
                    else if (c_cleaned.Equals("i", StringComparison.CurrentCultureIgnoreCase))
                    {
                        int argvalue_int = (int)argvalue;
                        bool result = add_int(argvalue_int);
                        if (result == false)
                        {
                            argHex = "";
                            return false;
                        }
                    }
                    else if (c_cleaned.Equals("s", StringComparison.CurrentCultureIgnoreCase))
                    {
                        short argvalue_short = (short)argvalue;
                        bool result = add_short(argvalue_short);
                        if (result == false)
                        {
                            argHex = "";
                            return false;
                        }
                    }
                    else if (c_cleaned.Equals("z"))
                    {
                        string argvalue_string = (string)argvalue;
                        bool result = add_string(argvalue_string);
                        if (result == false)
                        {
                            argHex = "";
                            return false;
                        }
                    }
                    else if (c_cleaned.Equals("Z"))
                    {
                        string argvalue_string = (string)argvalue;
                        bool result = add_wide_string(argvalue_string);
                        if (result == false)
                        {
                            argHex = "";
                            return false;
                        }
                    }
                    else
                    {
                        argHex = "";
                        Console.WriteLine("Invalid argTypes string, must be either b i s z Z");
                        return false;
                    }
                }
               
                buffer = Pack(buffer.Length,buffer);
                //argHex = BitConverter.ToString(buffer).Replace("-", "");
                argHex = Convert.ToBase64String(buffer);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error packing data: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                argHex = "";
                return false;
            }
            
        }

        public static byte[] Pack(int size, byte[] buffer)
        {
            // Get bytes of size in little endian format
            byte[] sizeBytes = BitConverter.GetBytes(size);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(sizeBytes);
            }

            // Concatenate sizeBytes and buffer
            return sizeBytes.Concat(buffer).ToArray();
        }
    }
}
