namespace HardHatC2Client.Utilities
{
    public class CommandOutputParsing
    {

        public static Dictionary<string,string> ParseSeatbelt(List<string> seatbeltContent)
        {
            try
            {
                var output = new Dictionary<string, string>();
                /*
                 * ====== Property Header  ======
                 * line 1 of text 
                 * line 2 of text 
                 * line 3 of text 
                   ====== Other Property ======
                 */

                //check too see if an array element contains ====== x  ====== , x can be any word or phrase, if it does this will become a key in the dictionary, if it doesn't it will be added to the value of the previous key
                foreach (var line in seatbeltContent)
                {
                    //skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    //if line says completed collection in x time then skip it
                    if (line.Contains("Completed collection in"))
                    {
                        continue;
                    }
                    if (line.Contains("======"))
                    {
                        var key = line.Replace("======", "").Trim();
                        output.Add(key, "");
                    }
                    else
                    {
                        //if the line doesn't contain ====== x  ====== then it will be added to the value of the previous key if there is not a pervious key then move to the next line
                        if (output.Count == 0)
                        {
                            continue;
                        }
                        else
                        {
                            var key = output.Keys.Last();
                            var value = output[key];
                            output[key] = value + line + Environment.NewLine;
                        }
                    }
                }
                //return the dictionary
                return output;
            }
            catch (Exception ex )
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return new Dictionary<string, string>();
            }
        }

    }
}
