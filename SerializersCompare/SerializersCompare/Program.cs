using Newtonsoft.Json;
using ProtoBuf;
using SerializersCompare.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Linq;

namespace SerializersCompare
{
    class Program
    {
        private static readonly String _workDirectoryPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/SerializersCompare";
        private static readonly Stopwatch _timer = new Stopwatch();
        private const Int32 PERSONS_NUMBER = 200000;


        static void Main(string[] args)
        {
            if (!Directory.Exists(_workDirectoryPath))
                Directory.CreateDirectory(_workDirectoryPath);

            Console.WriteLine("Processing... \r\n");
            var persons = GeneratePersons();

            var buffer = SerializeToProtobuf(persons);
            ShowResult("Protobuf", _timer.Elapsed, buffer.Length);
            WriteResultToFile($"{_workDirectoryPath}/personsProto.bin", buffer);

            buffer = SerializeToJson(persons);
            ShowResult("JSON", _timer.Elapsed, buffer.Length);
            WriteResultToFile($"{_workDirectoryPath}/persons.json", buffer);

            buffer = SerializeToSimpleBinary(persons);
            ShowResult("Binary", _timer.Elapsed, buffer.Length);
            WriteResultToFile($"{_workDirectoryPath}/personsSimple.dat", buffer);

            buffer = SerializeToXml(SaveOptions.DisableFormatting, persons);
            ShowResult("XML", _timer.Elapsed, buffer.Length);
            WriteResultToFile($"{_workDirectoryPath}/persons.xml", buffer);


            Console.WriteLine();


            buffer = SerializeToProtobufPlusZip("persons.bin", persons);
            ShowResult("Protobuf + zip", _timer.Elapsed, buffer.Length);
            WriteResultToFile($"{_workDirectoryPath}/personsProto.zip", buffer);

            buffer = SerializeToJsonPlusZip("persons.json", persons);
            ShowResult("JSON + zip", _timer.Elapsed, buffer.Length);
            WriteResultToFile($"{_workDirectoryPath}/personsJson.zip", buffer);

            buffer = SerializeToSimpleBinaryPlusZip("persons.dat", persons);
            ShowResult("Binary + zip", _timer.Elapsed, buffer.Length);
            WriteResultToFile($"{_workDirectoryPath}/personsSimple.zip", buffer);

            buffer = SerializeToXmlPlusZip("persons.xml", SaveOptions.DisableFormatting, persons);
            ShowResult("XML + zip", _timer.Elapsed, buffer.Length);
            WriteResultToFile($"{_workDirectoryPath}/personsXml.zip", buffer);


            Console.WriteLine("\r\nDone");
            Console.ReadKey();
        }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        private static List<Person> GeneratePersons()
        {
            var personsList = new List<Person>(PERSONS_NUMBER);
            var rnd = new Random();

            for (var i = 0; i < PERSONS_NUMBER; i++)
            {
                var personId = rnd.Next();
                personsList.Add(new Person
                {
                    Id = personId,
                    Name = $"Person {personId} name",
                    Phones = new Int32[] { rnd.Next(), rnd.Next(), rnd.Next() },
                    Address = new Address
                    {
                        Value1 = rnd.Next(),
                        Value2 = rnd.NextDouble(),
                        Value3 = rnd.Next() > Int32.MaxValue / 2
                    }
                });
            }
            return personsList;
        }
        private static Byte[] SerializeToProtobuf(List<Person> persons)
        {
            using (var memoryStream = new MemoryStream())
            {
                _timer.Restart();
                Serializer.Serialize(memoryStream, persons);
                _timer.Stop();
                return memoryStream.ToArray();
            }
        }
        private static Byte[] SerializeToJson(List<Person> persons)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    _timer.Restart();
                    writer.Write(JsonConvert.SerializeObject(persons));
                    _timer.Stop();
                    return memoryStream.ToArray();
                }
            }
        }
        private static Byte[] SerializeToSimpleBinary(List<Person> persons)
        {
            using (var memoryStream = new MemoryStream())
            {
                _timer.Restart();
                new BinaryFormatter().Serialize(memoryStream, persons);
                _timer.Stop();
                return memoryStream.ToArray();
            }
        }
        private static Byte[] SerializeToXml(SaveOptions saveOptions, List<Person> persons)
        {
            using (var memoryStream = new MemoryStream())
            {
                _timer.Restart();
                GenerateXml(memoryStream, saveOptions, persons);
                _timer.Stop();
                return memoryStream.ToArray();
            }
        }
        private static Byte[] SerializeToProtobufPlusZip(String entryFileName, List<Person> persons)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
                    var zipStream = zipArchiveEntry.Open();
                    _timer.Restart();
                    Serializer.Serialize(zipStream, persons);
                    _timer.Stop();

                    return memoryStream.ToArray();
                }
            }
        }
        private static Byte[] SerializeToJsonPlusZip(String entryFileName, List<Person> persons)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
                    using (var writer = new StreamWriter(zipArchiveEntry.Open(), Encoding.UTF8))
                    {
                        _timer.Restart();
                        writer.Write(JsonConvert.SerializeObject(persons));
                        _timer.Stop();

                        return memoryStream.ToArray();
                    }
                }
            }
        }
        private static Byte[] SerializeToSimpleBinaryPlusZip(String entryFileName, List<Person> persons)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
                    _timer.Restart();
                    new BinaryFormatter().Serialize(zipArchiveEntry.Open(), persons);
                    _timer.Stop();

                    return memoryStream.ToArray();
                }
            }
        }
        private static Byte[] SerializeToXmlPlusZip(String entryFileName, SaveOptions saveOptions, List<Person> persons)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
                    _timer.Restart();
                    GenerateXml(zipArchiveEntry.Open(), saveOptions, persons);
                    _timer.Stop();

                    return memoryStream.ToArray();
                }
            }
        }


        // SUPPORT FUNCTIONS //////////////////////////////////////////////////////////////////////
        private static void WriteResultToFile(String filePath, Byte[] buffer)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }
        private static T DeserializeSimpleBinary<T>(String filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                return (T)new BinaryFormatter().Deserialize(fileStream);
            }
        }
        private static void GenerateXml(Stream memoryStream, SaveOptions saveOptions, List<Person> persons)
        {
            var xDoc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", "yes"),
                    new XDocumentType("Persons", null, "Persons.dtd", null),
                    new XProcessingInstruction("PersonCataloger", "out-of-print"));

            var personElements = new XElement("persons");
            foreach (var x in persons)
            {
                var personElement = new XElement("person",
                    new XAttribute("id", x.Id),
                    new XAttribute("name", x.Name),
                    new XElement("address",
                        new XElement("value1", x.Address.Value1),
                        new XElement("value2", x.Address.Value2),
                        new XElement("value3", x.Address.Value3)));

                var personPhonesElement = new XElement("phones");
                foreach (var y in x.Phones)
                {
                    personPhonesElement.Add(
                        new XElement("phone",
                            new XText(y.ToString())));
                }
                personElements.Add(personElement);
            }
            xDoc.Add(personElements);
            xDoc.Save(memoryStream, saveOptions);
        }
        private static void ShowResult(String methodName, TimeSpan elapsedTime, Int64 resultSize)
        {
            Console.WriteLine($"{methodName,-15}\t{elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}.{elapsedTime.Milliseconds / 10:00}\t{resultSize / 1000:###.###.###} KB");
        }
    }
}