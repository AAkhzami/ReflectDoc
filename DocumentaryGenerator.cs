using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DocumentaryGeneratorClass
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class DecriptionAttribute : Attribute
    {
        public string Description { get; }
        public DecriptionAttribute(string description)
        {
            this.Description = description;
        }
    }
    public class DocumentaryGenerator
    {
        Type Type;
        string GetInterfaceList(Type[] Interfaces)
        {
            return string.Join(" - ", Interfaces.Select(Interface => $"  Interface Name : {Interface.Name}.\n   * Source :{Interface.FullName}\n"));
        }
        string GetModifiersAttributesInfo(Type type)
        {
            string Text = "";

            Text += "* Access : ";
            Text += (type.IsPublic) ? "**Public**\n\n" : "**Not Public**\n\n";
            Text += (type.IsAbstract) ? "**  Note: This class is a Base Class and cannot be instantiated directly using new. It serves as a template for other classes.**\n" : "";

            Text += "**Implemented Interfaces:**\n\n";
            if (type.GetInterfaces().Length > 0)
            {
                Text += GetInterfaceList(type.GetInterfaces());
                Text += "\n\n";
            }
            else
                Text += ">**No Interface Implemented!**\n\n";
            return Text;
        }
        string GetParameterList(ParameterInfo[] parameters)
        {
            if (parameters.Length == 0)
                return "No Parameters";
            return string.Join(", ", parameters.Select(parameter => $"{parameter.ParameterType.Name} {parameter.Name}"));
        }
        string GetModifiersList(MethodInfo t)
        {
            string Text = "";

            Text += t.IsPublic ? "Public" : "";
            Text += t.IsPrivate ? "Private" : "";
            Text += t.IsFamily ? "protected" : "";
            Text += t.IsAssembly ? "internal" : "";

            Text += t.IsStatic ? ", Static" : "";
            Text += t.IsVirtual ? ", Virtual" : "";
            Text += t.IsAbstract ? ", Abstract" : "";
            Text += t.IsFinal ? ", Final" : "";

            return Text;
        }
        string GetDescriptionMethodAttribute(MethodInfo t)
        {
            string Text = "";
            object[] attributes = t.GetCustomAttributes(typeof(DecriptionAttribute), false);

            if (attributes.Length > 0)
            {
                foreach (DecriptionAttribute attribute in attributes)
                {
                    Text += attribute.Description + ".";
                }
            }
            else
                Text = "No Description.";

            return Text;
        }
        string GetDescriptionAttribute(Type t)
        {
            string Text = "";
            object[] attributes = t.GetCustomAttributes(typeof(DecriptionAttribute), false);
            if (attributes.Length > 0)
            {
                foreach (DecriptionAttribute attribute in attributes)
                {
                    Text += attribute.Description + ".";
                }
            }
            else
                Text = "No Description!";

            return Text;
        }
        string GetMethodsInfo(MethodInfo[] Methods)
        {
            string Text = null;
            int Counter = 1;
            foreach (var typeMethod in Methods)
            {
                if (!typeMethod.IsSpecialName)
                {
                    Text += $" **{Counter}. {typeMethod.Name}**\n";
                    Text += $"  * Return Type: `{typeMethod.ReturnType.Name}`\n";
                    Text += $"  * Parameters : `{GetParameterList(typeMethod.GetParameters())}`\n";
                    Text += $"  * Modifiers  : `{GetModifiersList(typeMethod)}`\n";
                    Text += $"  * Description:\n     **{GetDescriptionMethodAttribute(typeMethod)}**\n\n";
                    Counter++;
                }

            }

            Text += "\n\n";
            return Text;
        }
        string GetPropertiesInfo(PropertyInfo[] Properties)
        {
            string Text = null;
            Text += "   | Name | Type | Access|\n";
            Text += "   | :--- | :--- | :--- |\n";
            foreach (var typePropertie in Properties)
            {
                string Access = "";
                Access += typePropertie.CanRead ? "Get;" : "";
                Access += typePropertie.CanWrite ? "Set; " : "";
                Text += $"| {typePropertie.Name} | {typePropertie.PropertyType.Name} | {Access} |\n";
            }
            Text += "\n\n";
            return Text;
        }
        string GetFieldsInfo(FieldInfo[] Fields)
        {
            string Text = null;
            Text += "| Name | Type | Info |\n";
            Text += "| :--- | :--- | :--- |\n";
            foreach (var typeField in Fields)
            {
                if (typeField.Name.StartsWith("<")) continue;
                string moreInfo = "";
                moreInfo += typeField.IsInitOnly ? ",readonly" : "";
                moreInfo += typeField.IsLiteral ? ",const" : "";
                Text += $"| {typeField.Name} | {typeField.FieldType.Name} | ";
                Text += typeField.IsPublic ? "Public" : "";
                Text += typeField.IsPrivate ? "Private" : "";
                Text += typeField.IsFamily ? "Protected" : "";
                Text += moreInfo + " |\n";
            }
            Text += "\n\n";
            return Text;
        }


        string GetConstructorsInfo(ConstructorInfo[] constructors)
        {
            string Text = null;

            if (constructors.Length > 0)
            {
                int counter = 1;
                foreach (var constructor in constructors)
                {
                    string Access = "";
                    Access = constructor.IsPublic ? "Public" : "Private";
                    Text += $" * {constructor.DeclaringType.Name} `Constructor` ({Access})\n   **Parameters:{GetParameterList(constructor.GetParameters())}**\n";

                    counter++;
                }
            }
            else
                Text = "**No Constractor**";

            return Text;
        }
        public string CreateDocumentary(Type type)
        {
            string md = null;

            if (type != null)
            {
                md += $"## Documentation for {type.Name}\n\n";
                md += $"### Class FullName : {type.FullName}\n\n";
                md += $"> The class name is '{type.Name}'. Is Currently defined in '{type.Module}'. ";

                md += $"{GetDescriptionAttribute(type)}\n\n";

                Type InheritedFrom = type.BaseType;
                if (InheritedFrom.ToString() != "System.Object")
                    md += $"> Is Inherited from:  {InheritedFrom.Name}, That Located in ({InheritedFrom}).\n\n";

                md += "### Modifiers & Attributes\n\n";
                md += GetModifiersAttributesInfo(type);

                md += $" **Methods Count      : {type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Length}** `Note: The methods count may includes compiler-generated methods for properties (getters & setters)` \n\n";
                md += $" **Properties Count   : {type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Length}** \n\n";
                md += $" **Fields Count       : {type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Length}** \n\n";
                md += $" **Constructors Count : {type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Length}** \n\n";

                md += "### 🔑  Properties\n\n";
                var Properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                md += GetPropertiesInfo(Properties);


                md += "### 🔑 Properties\n\n";
                var Fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                md += GetFieldsInfo(Fields);


                md += "### 🛠 Methods\n\n";
                var Methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                md += GetMethodsInfo(Methods);


                md += "### CONSTRUCTORS\n\n";
                var Constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                md += GetConstructorsInfo(Constructors);
            }

            return md;

        }
        public DocumentaryGenerator(Type T)
        {
            Type = T;
        }
        public DocumentaryGenerator()
        { }
        public bool CreateClassDocumenrty()
        {
            string MarkdownResult = CreateDocumentary(Type);
            if (MarkdownResult != null)
            {
                string fileName = $"{Type.Name}.md";
                File.WriteAllText(fileName, MarkdownResult);
                return true;
            }
            else
                return false;
                
        }
        public bool CreateClassDocumenrty(Type t)
        {
            string MarkdownResult = CreateDocumentary(t);
            if (MarkdownResult != null)
            {
                string fileName = $"{t.Name}.md";
                File.WriteAllText(fileName, MarkdownResult);
                return true;
            }
            else
                return false;

        }
        static public void CreateAssemblyDocumenrty(Assembly assembly, string FileName = null)
        {
            Type[] types = assembly.GetTypes();
            DocumentaryGenerator dg = new DocumentaryGenerator();
            StringBuilder fullContent = new StringBuilder();

            fullContent.AppendLine("# Full Project Documentation");
            fullContent.AppendLine($"> Generated on: {DateTime.Now}\n");
            fullContent.AppendLine("---" + Environment.NewLine);


            foreach (Type t in types)
            {
                if (!t.IsPublic) continue;
                if (t == typeof(DecriptionAttribute)) continue;

                if (t.IsClass)
                {
                    string classDoc = dg.CreateDocumentary(t);
                    fullContent.AppendLine(classDoc + Environment.NewLine);
                    fullContent.AppendLine("---");
                }
            }
            string filePath = FileName ?? "AssemblyDocumentary.md";

            File.WriteAllText(filePath, fullContent + Environment.NewLine);
        }
        static public void CreateExternalAssemblyDocumenrty(string dllFile, string FileName = null)
        {
            Assembly externalAssembly = Assembly.LoadFrom(dllFile);
            CreateAssemblyDocumenrty(externalAssembly, FileName);
        }
    }

}
