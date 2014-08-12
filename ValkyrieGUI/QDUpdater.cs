using System;
using System.Data;
using System.Configuration;
using System.Collections;

namespace ValkyrieGUI
{
	/// <summary>
	/// Legacy .Net 1.1 utility classes for SQL access.  Should be burned and replaced.  Only used
	/// in test code now.
	/// </summary>

    public class QDUpdater
    {
        public Hashtable ht = new Hashtable();
        public string tableName = "";
        public int lastInsertID = -1;
        public string condition = "";
        public QDUpdater(string table, string cond)
        {
            tableName = table;
            condition = cond;
        }
        public void update(string field, string value, Types type)
        {
            if (type == Types.String || type == Types.Date)
            {
                if (type == Types.String)
                {
                    value = value.Replace("'", "''");
                }
                value = "'" + value + "'";
            }
            ht[field] = value;
        }
        public string this[string field, Types type]
        {
            //get { return m_matrix[x, y]; }
            set { update(field, value, type); }
        }

    }

}