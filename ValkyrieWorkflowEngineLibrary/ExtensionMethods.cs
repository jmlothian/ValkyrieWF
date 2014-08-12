using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ValkyrieWorkflowEngineLibrary
{
    public static class ExtensionMethods
    {
        // Deep clone
		// TODO: this is really a slow way to handle this.  We don't clone in many places, 
		//       so we should just write the functions to clone without serialization
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
