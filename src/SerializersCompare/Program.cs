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
		private static readonly String _workDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SerializersCompare");
		private static readonly Stopwatch _timer = new Stopwatch();
		private const Int32 _personsAmount = 100000;


		static void Main(String[] args)
		{
			if(!Directory.Exists(_workDirectoryPath))
				Directory.CreateDirectory(_workDirectoryPath);

			Console.WriteLine("Processing... \r\n");
			var persons = GeneratePersons();

			var buffer = SerializeToProtobuf(persons);
			ShowResult("Protobuf", _timer.Elapsed, buffer.Length);
			WriteResultToFile($"{_workDirectoryPath}/personsProto.bin", buffer);

			buffer = SerializeToJsonByNewtonsoft(persons);
			ShowResult("Newtonsoft JSON", _timer.Elapsed, buffer.Length);
			WriteResultToFile($"{_workDirectoryPath}/newtonsoftPersons.json", buffer);

			buffer = SerializeToJsonByMicrosoft(persons);
			ShowResult("Microsoft JSON", _timer.Elapsed, buffer.Length);
			WriteResultToFile($"{_workDirectoryPath}/microsoftPersons.json", buffer);

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

			buffer = SerializeToJsonByNewtonsoftPlusZip("persons.json", persons);
			ShowResult("N JSON + zip", _timer.Elapsed, buffer.Length);
			WriteResultToFile($"{_workDirectoryPath}/newtonsoftPersonsJson.zip", buffer);

			buffer = SerializeToJsonByMicrosoftPlusZip("persons.json", persons);
			ShowResult("M JSON + zip", _timer.Elapsed, buffer.Length);
			WriteResultToFile($"{_workDirectoryPath}/microsoftPersonsJson.zip", buffer);

			buffer = SerializeToSimpleBinaryPlusZip("persons.bin", persons);
			ShowResult("Binary + zip", _timer.Elapsed, buffer.Length);
			WriteResultToFile($"{_workDirectoryPath}/personsSimpleBinary.zip", buffer);

			buffer = SerializeToXmlPlusZip("persons.xml", SaveOptions.DisableFormatting, persons);
			ShowResult("XML + zip", _timer.Elapsed, buffer.Length);
			WriteResultToFile($"{_workDirectoryPath}/personsXml.zip", buffer);


			Console.WriteLine("\r\nDone");
			Console.ReadKey();
		}


		// FUNCTIONS //////////////////////////////////////////////////////////////////////////////
		private static Person[] GeneratePersons()
		{
			var personsList = new List<Person>(_personsAmount);
			var rnd = new Random();

			for(var i = 0; i < _personsAmount; i++)
			{
				var personId = rnd.Next();
				personsList.Add(new Person
				{
					Id = personId,
					TransportId = Guid.NewGuid(),
					Name = $"Person {personId} name",
					SequenceId = i,
					CreditCards = new[] { rnd.Next(), rnd.Next(), rnd.Next() },
					Age = rnd.Next(100),
					Phones = new[] { rnd.Next().ToString(), rnd.Next().ToString(), rnd.Next().ToString() },
					BirthDate = DateTime.UtcNow + TimeSpan.FromTicks(rnd.Next()),
					Salary = rnd.NextDouble(),
					IsMarred = rnd.Next() > Int32.MaxValue / 2
				});
			}
			return personsList.ToArray();
		}
		private static Byte[] SerializeToProtobuf(Person[] persons)
		{
			using(var memoryStream = new MemoryStream())
			{
				_timer.Restart();
				Serializer.Serialize(memoryStream, persons);
				_timer.Stop();
				return memoryStream.ToArray();
			}
		}
		private static Byte[] SerializeToJsonByNewtonsoft(Person[] persons)
		{
			using(var memoryStream = new MemoryStream())
			{
				using(var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
				{
					using(var textWriter = new JsonTextWriter(streamWriter))
					{
						_timer.Restart();
						new JsonSerializer().Serialize(textWriter, persons);
						_timer.Stop();

						return memoryStream.ToArray();
					}
				}
			}
		}
		private static Byte[] SerializeToJsonByMicrosoft(Person[] persons)
		{
			using(var memoryStream = new MemoryStream())
			{
				using(var jsonWriter = new System.Text.Json.Utf8JsonWriter(memoryStream))
				{
					_timer.Restart();
					System.Text.Json.JsonSerializer.Serialize(jsonWriter, persons);
					_timer.Stop();

					return memoryStream.ToArray();
				}
			}
		}
		private static Byte[] SerializeToSimpleBinary(Person[] persons)
		{
			using(var memoryStream = new MemoryStream())
			{
				_timer.Restart();
				new BinaryFormatter().Serialize(memoryStream, persons);
				_timer.Stop();
				return memoryStream.ToArray();
			}
		}
		private static Byte[] SerializeToXml(SaveOptions saveOptions, Person[] persons)
		{
			using(var memoryStream = new MemoryStream())
			{
				_timer.Restart();
				GenerateXml(memoryStream, saveOptions, persons);
				_timer.Stop();
				return memoryStream.ToArray();
			}
		}
		private static Byte[] SerializeToProtobufPlusZip(String entryFileName, Person[] persons)
		{
			using(var memoryStream = new MemoryStream())
			{
				using(var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
				{
					var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
					var zipStream = zipArchiveEntry.Open();
					_timer.Restart();
					Serializer.Serialize(zipStream, persons);
					_timer.Stop();
				}
				return memoryStream.ToArray();
			}
		}
		private static Byte[] SerializeToJsonByNewtonsoftPlusZip(String entryFileName, Person[] persons)
		{
			using(var memoryStream = new MemoryStream())
			{
				using(var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
				{
					var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
					using(var entry = zipArchiveEntry.Open())
					{
						using(var streamWriter = new StreamWriter(entry, Encoding.UTF8))
						{
							using(var textWriter = new JsonTextWriter(streamWriter))
							{
								_timer.Restart();
								new JsonSerializer().Serialize(textWriter, persons);
								_timer.Stop();
							}
						}
					}
				}
				return memoryStream.ToArray();
			}
		}
		private static Byte[] SerializeToJsonByMicrosoftPlusZip(String entryFileName, Person[] persons)
		{
			using(var memoryStream = new MemoryStream())
			{
				using(var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
				{
					var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
					using(var entry = zipArchiveEntry.Open())
					{
						using(var jsonWriter = new System.Text.Json.Utf8JsonWriter(entry))
						{
							_timer.Restart();
							System.Text.Json.JsonSerializer.Serialize(jsonWriter, persons);
							_timer.Stop();


						}
					}
				}
				return memoryStream.ToArray();
			}
		}
		private static Byte[] SerializeToSimpleBinaryPlusZip(String entryFileName, Person[] persons)
		{
			using(var memoryStream = new MemoryStream())
			{
				using(var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
				{
					var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
					_timer.Restart();
					new BinaryFormatter().Serialize(zipArchiveEntry.Open(), persons);
					_timer.Stop();
				}
				return memoryStream.ToArray();
			}
		}
		private static Byte[] SerializeToXmlPlusZip(String entryFileName, SaveOptions saveOptions, Person[] persons)
		{
			using(var memoryStream = new MemoryStream())
			{
				using(var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
				{
					var zipArchiveEntry = zipArchive.CreateEntry(entryFileName, CompressionLevel.Optimal);
					_timer.Restart();
					GenerateXml(zipArchiveEntry.Open(), saveOptions, persons);
					_timer.Stop();
				}
				return memoryStream.ToArray();
			}
		}


		// SUPPORT FUNCTIONS //////////////////////////////////////////////////////////////////////
		private static void WriteResultToFile(String filePath, Byte[] buffer)
		{
			using(var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				fileStream.Write(buffer, 0, buffer.Length);
			}
		}
		private static T DeserializeSimpleBinaryFromFile<T>(String filePath)
		{
			using(var fileStream = new FileStream(filePath, FileMode.Open))
			{
				return (T)new BinaryFormatter().Deserialize(fileStream);
			}
		}
		private static void GenerateXml(Stream memoryStream, SaveOptions saveOptions, Person[] persons)
		{
			var xDoc = new XDocument(
					new XDeclaration("1.0", "UTF-8", "yes"),
					new XDocumentType("Persons", null, "Persons.dtd", null),
					new XProcessingInstruction("PersonCataloger", "out-of-print"));

			var personElements = new XElement("persons");
			foreach(var x in persons)
			{
				var personElement = new XElement("person",
					new XAttribute("id", x.Id),
					new XAttribute("transportId", x.TransportId.ToString()),
					new XAttribute("name", x.Name),
					new XAttribute("ancestorId", x.SequenceId),
					new XAttribute("age", x.Age),
					new XAttribute("birthDate", x.BirthDate),
					new XAttribute("salary", x.Salary),
					new XAttribute("isMarred", x.IsMarred));

				var creditCardsElement = new XElement("creditCards");
				foreach(var y in x.CreditCards)
				{
					creditCardsElement.Add(new XElement("creditCard",
						new XText(y.ToString())));
				}
				personElements.Add(creditCardsElement);

				var personPhonesElement = new XElement("phones");
				foreach(var y in x.Phones)
				{
					personPhonesElement.Add(new XElement("phone",
						new XText(y)));
				}
				personElement.Add(personPhonesElement);
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
