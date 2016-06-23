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
        private const Int32 PERSONS_NUMBER = 200000;


        static void Main(string[] args)
        {
            if (!Directory.Exists(_workDirectoryPath))
                Directory.CreateDirectory(_workDirectoryPath);

            Console.WriteLine("Processing... \r\n");
            var timer = new Stopwatch();
            var persons = GeneratePersons();

            timer.Restart();
            SerializeToProtobuf($"{_workDirectoryPath}/personsProto.bin", persons);
            timer.Stop();
            ShowElapsedTime("Protobuf", timer.Elapsed);

            timer.Restart();
            SerializeToJson($"{_workDirectoryPath}/persons.json", persons);
            timer.Stop();
            ShowElapsedTime("JSON", timer.Elapsed);

            timer.Restart();
            SerializeToSimpleBinary($"{_workDirectoryPath}/personsSimple.dat", persons);
            timer.Stop();
            ShowElapsedTime("Binary", timer.Elapsed);

            timer.Restart();
            SerializeToXml($"{_workDirectoryPath}/persons.xml", SaveOptions.DisableFormatting, persons);
            timer.Stop();
            ShowElapsedTime("XML", timer.Elapsed);

            Console.WriteLine();

            timer.Restart();
            SerializeToProtobufPlusZip($"{_workDirectoryPath}/personsProto.zip", "persons.bin", persons);
            timer.Stop();
            ShowElapsedTime("Protobuf + zip", timer.Elapsed);

            timer.Restart();
            SerializeToJsonPlusZip($"{_workDirectoryPath}/personsJson.zip", "persons.json", persons);
            timer.Stop();
            ShowElapsedTime("JSON + zip", timer.Elapsed);

            timer.Restart();
            SerializeToSimpleBinaryPlusZip($"{_workDirectoryPath}/personsSimple.zip", "persons.dat", persons);
            timer.Stop();
            ShowElapsedTime("Binary + zip", timer.Elapsed);

            timer.Restart();
            SerializeToXmlPlusZip($"{_workDirectoryPath}/personsXml.zip", "persons.xml", SaveOptions.DisableFormatting, persons);
            timer.Stop();
            ShowElapsedTime("XML + zip", timer.Elapsed);

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
        private static void SerializeToProtobuf(String filePath, List<Person> persons)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                Serializer.Serialize(fileStream, persons);
            }
        }
        private static void SerializeToProtobufPlusZip(String filePath, String entryFileName, List<Person> persons)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
                    var zipStream = zipArchiveEntry.Open();
                    Serializer.Serialize(zipStream, persons);
                }
            }
        }
        private static void SerializeToJson(String filePath, List<Person> persons)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    writer.Write(JsonConvert.SerializeObject(persons));
                }
            }
        }
        private static void SerializeToJsonPlusZip(String filePath, String entryFileName, List<Person> persons)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
                    using (var writer = new StreamWriter(zipArchiveEntry.Open(), Encoding.UTF8))
                    {
                        writer.Write(JsonConvert.SerializeObject(persons));
                    }
                }
            }
        }
        private static void SerializeToSimpleBinary(String filePath, List<Person> persons)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                new BinaryFormatter().Serialize(fileStream, persons);
            }
        }
        private static void SerializeToSimpleBinaryPlusZip(String filePath, String entryFileName, List<Person> persons)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
                    new BinaryFormatter().Serialize(zipArchiveEntry.Open(), persons);
                }
            }
        }
        private static void SerializeToXml(String filePath, SaveOptions saveOptions, List<Person> persons)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                GenerateXml(fileStream, saveOptions, persons);
            }
        }
        private static void SerializeToXmlPlusZip(String filePath, String entryFileName, SaveOptions saveOptions, List<Person> persons)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
                    GenerateXml(zipArchiveEntry.Open(), saveOptions, persons);
                }
            }
        }


        // SUPPORT FUNCTIONS //////////////////////////////////////////////////////////////////////
        private static T DeserializeSimpleBinary<T>(String filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                return (T)new BinaryFormatter().Deserialize(fileStream);
            }
        }
        private static void GenerateXml(Stream fileStream, SaveOptions saveOptions, List<Person> persons)
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
            xDoc.Save(fileStream, saveOptions);
        }
        private static void ShowElapsedTime(String methodName, TimeSpan elapsedTime)
        {
            Console.WriteLine($"{methodName,-15}\t{elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}.{elapsedTime.Milliseconds / 10:00}");
        }
    }
}