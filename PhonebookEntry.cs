using System;
using System.Text.RegularExpressions;

namespace Phonebook
{
	public class PhonebookEntry
	{
		private string m_id, m_surname, m_firstname, m_phone, m_address;
		private const char DELIMITER = '|';
		
		public PhonebookEntry(string surname, string firstname, string phone, string address)
		{
			m_id = Guid.NewGuid().ToString().ToUpper();
			m_surname = MakeSafe(surname);
			m_firstname = MakeSafe(firstname);
			m_phone = MakeSafe(phone);
			m_address = MakeSafe(address);
		}
		
		public PhonebookEntry(string id, string surname, string firstname, string phone, string address)
		{
			m_id = id;
			m_surname = surname;
			m_firstname = firstname;
			m_phone = phone;
			m_address = address;			
		}		
		
		public PhonebookEntry(string line, out string error)
		{
			error = "";
			string[] attributes = line.Split(DELIMITER);
			if (attributes.Length != 5)
				error = "Invalid entry";
			else
			{
				m_id = attributes[0];
				m_surname = attributes[1];
				m_firstname = attributes[2];
				m_phone = attributes[3];
				m_address = attributes[4];				
			}
		}		
		
		public string Id
		{
			get
			{
				return m_id;
			}
		}					

		public string Surname
		{
			get
			{
				return m_surname;
			}
		}
		
		public string Firstname
		{
			get
			{
				return m_firstname;
			}
		}		
		
		public string Phone
		{
			get
			{
				return m_phone;
			}
		}				
		
		public string Address
		{
			get
			{
				return m_address;
			}
		}
		
		public string GetLine()
		{
			return String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", m_id, DELIMITER, m_surname, DELIMITER, m_firstname, DELIMITER, m_phone, DELIMITER, m_address);
		}
		
		private string MakeSafe(string value)
		{
			value = value.Replace(DELIMITER, ' ');
			value = Regex.Replace(value, @"\r\n?|\n", " ");
			
			return value;
		}
	}
}
