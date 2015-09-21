using System;
using System.IO;
using System.Text;

namespace Phonebook
{
	public class PhonebookStore
	{	
		public PhonebookStore()
		{			
		}
		
		public bool Load(Phonebook phonebook, out string error)
		{			
			error = "";
			try
			{
				using (StreamReader reader = new StreamReader(GetFilename(), Encoding.UTF8))
				{
					while (reader.Peek() >= 0)
					{
						string line = reader.ReadLine();
						PhonebookEntry entry = new PhonebookEntry(line, out error);
						if (error.Length > 0)
						{
							error = "At least one entry in phonebook is invalid";
							return false;
						}
						phonebook.AddEntry(entry);
					}
				}
			}
			catch (FileNotFoundException)
			{
				// file not found just means phonebook is empty
			}
			catch (IOException)
			{
				error = "Problem accessing phonebook";
				return false;
			}
			catch (OutOfMemoryException)
			{
				error = "Ran out of memory";
				return false;
			}
						
			return true;
		}
			
		public bool Save(Phonebook phonebook, out string error)
		{
			error = "";
			try
			{
				using (StreamWriter writer = new StreamWriter(GetFilename(), false, Encoding.UTF8))
				{
					foreach (PhonebookEntry entry in phonebook)
					{
						writer.WriteLine(entry.GetLine());
					}
				}
			}
			catch (UnauthorizedAccessException)
			{
				error = "Access denied";
				return false;
			}
			catch (DirectoryNotFoundException)
			{
				error = "Directory not found";
				return false;
			}
			
			return true;
		}
		
		private string GetFilename()
		{
			return Path.Combine(Path.GetTempPath(), "phonebook.txt");
		}		
	}
}
