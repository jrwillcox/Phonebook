using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Phonebook
{
	public class Phonebook : IEnumerator, IEnumerable
	{		
		List<PhonebookEntry> m_Phonebook;
		int m_Position = -1;
		PhonebookStore m_PhonebookStore;
		
		public Phonebook()
		{
			m_Phonebook = new List<PhonebookEntry>();
			m_PhonebookStore = new PhonebookStore();						
		}
		
		public IEnumerator GetEnumerator()
		{
			return (IEnumerator)this;
		}
				
		public bool MoveNext()
		{
			m_Position++;
			return (m_Position < m_Phonebook.Count);
		}
		
		public void Reset()
		{
			m_Position = 0;
		}
		
		public object Current
		{
			get 
			{ 
				return m_Phonebook[m_Position]; 
			}
		}
					
		public bool AddEntry(PhonebookEntry entry)
		{
			m_Phonebook.Add(entry);
			
			return true;
		}
		
		public bool RemoveEntry(string id)
		{
			int index = GetEntryIndexById(id);
			if (index == -1)
				return false;
			
			m_Phonebook.RemoveAt(GetEntryIndexById(id));				
			
			return true;
		}
		
		public bool UpdateEntry(PhonebookEntry entry)
		{			
			int i = GetEntryIndexById(entry.Id);
			if (i == -1)
				return false;
			
			m_Phonebook[GetEntryIndexById(entry.Id)] = entry;			
			
			return true;
		}
		
		public bool Load(out string error)
		{
			return m_PhonebookStore.Load(this, out error);
		}
		
		public bool Save(out string error)
		{
			return m_PhonebookStore.Save(this, out error);		
		}
		
		private int GetEntryIndexById(string id)
		{
			for (int e = 0; e < m_Phonebook.Count; e++)
			{
				if (m_Phonebook[e].Id == id)
					return e;
			}
			
			return -1;
		}
	}
}
