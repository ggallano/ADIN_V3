using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace JsonRegisterCombine
{
    internal class Program
    {
        private static uint _countRegTotal;
        private static uint _countRegAdded;
        private static uint _countRegDuplicate;
        private static RegisterSet registerSet = new RegisterSet() { Registers = new ObservableCollection<RegisterModel>() };

        static void Main(string[] args)
        {
            Console.WriteLine("Place all register map json files inside the \"RegMap_Separate\" folder.");
            Console.WriteLine("Press any key to continue...\n");
            Console.ReadKey(true);

            string[] jsonFilePaths = Directory.GetFiles("../../../RegMap_Separate", "*.json");

            foreach (string jsonFilePath in jsonFilePaths)
            {
                Console.Write($"File found \"{Path.GetFileName(jsonFilePath)}\"\n");
                Console.Write($"Adding registers... ");
                DeserializeFromJson(jsonFilePath, registerSet);
                _countRegTotal += _countRegAdded;
                Console.Write("Done!\n");
                Console.Write($"Total Registers     : {_countRegTotal}\n");
                Console.Write($"Added Registers     : {_countRegAdded}\n");
                Console.Write($"Duplicate Registers : {_countRegDuplicate}\n\n");
            }

            Console.Write("Type file name for combined json register map: ");
            string json_FileName = Console.ReadLine();

            Console.Write("Generating combined json register map at \"RegMap_Combined\"... ");
            string json_regMap = SerializeToJson(registerSet, 4);
            File.WriteAllText($"../../../RegMap_Combined/{json_FileName}", json_regMap);
            Console.Write("Done!\n");
            Console.Write($"Total Registers     : {_countRegTotal}\n");
            Console.Write("Press any key to end program.");
            Console.ReadKey();
        }

        public static void DeserializeFromJson(string regMapFileName, RegisterSet registerSet)
        {
            _countRegAdded = 0;
            _countRegDuplicate = 0;

            var registerStruct = JsonConvert.DeserializeObject<RegisterSet>(File.ReadAllText($"{regMapFileName}"));

            foreach (var register in registerStruct.Registers)
            {
                var matchRegName = registerSet.Registers.Where(x => x.Name == register.Name).ToList();

                if (matchRegName.Count > 0)
                    _countRegDuplicate++;
                else
                {
                    registerSet.Registers.Add(register);
                    _countRegAdded++;
                }
            }
        }

        public static string SerializeToJson(object obj, int indentationLength)
        {
            var stringWriter = new StringWriter();
            var jsonWriter = new JsonTextWriter(stringWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = indentationLength
            };

            var serializer = new JsonSerializer();
            serializer.Serialize(jsonWriter, obj);

            return stringWriter.ToString();
        }
    }
}

public class RegisterSet
{
    public ObservableCollection<RegisterModel> Registers { get; set; }
}

public class BitFieldModel
{
    public bool IncludeInDump { get; set; }
    public string Name { get; set; }
    public uint Start { get; set; }
    public uint Width { get; set; }
    public uint ResetValue { get; set; }
    public string Access { get; set; }
    public string MMap { get; set; }
    public uint Value { get; set; }
    public string Documentation { get; set; }
    public string Visibility { get; set; }
}

public class RegisterModel
{
    public bool IncludeInDump { get; set; }
    public string Name { get; set; }
    public uint Address { get; set; }
    public uint ResetValue { get; set; }
    public string MMap { get; set; }
    public string Documentation { get; set; }
    public string Visibility { get; set; }
    public string Image { get; set; }
    public string Access { get; set; }
    public string Group { get; set; }
    public List<BitFieldModel> BitFields { get; set; }
}