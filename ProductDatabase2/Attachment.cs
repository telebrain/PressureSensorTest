using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductDatabase2
{
    public class Attachment
    {
        public string Name { get; private set; }
        public TypeContentEnum TypeCont { get; private set; }
        public byte[] AttachContent { get; private set; }
        public byte[] FileContent { get; private set; }

        //const int LengthInfo = 22;
        readonly byte[] TextInfo = new byte[20] { 20, 0, 0, 0, 1, 0, 0, 0, 4, 0, 0, 0, 120, 0, 109, 0, 108, 0, 0, 0 };
        readonly byte[] XlsInfo = new byte[22] { 20, 0, 0, 0, 1, 0, 0, 0, 5, 0, 0, 0, 120, 0, 108, 0, 115, 0, 120, 0, 0, 0 };

        public Attachment(string name, TypeContentEnum typeContent, byte[] file)
        {
            Name = name;
            TypeCont = typeContent;
            FileContent = file;
            FileToAttachment();
        }

        public Attachment(string name, byte[] attachment)
        {
            Name = name;
            AttachContent = attachment;
            AttachmentToFile();
        }

        private void FileToAttachment()
        {
            byte[] prefix = null;
            switch (TypeCont)
            {
                case TypeContentEnum.text:
                    prefix = TextInfo;
                    break;
                case TypeContentEnum.xlsx:
                    prefix = XlsInfo;
                    break;
            }
            byte[] attachContent = new byte[prefix[0] + FileContent.Length];
            prefix.CopyTo(attachContent, 0);
            FileContent.CopyTo(attachContent, prefix[0]);
            AttachContent = attachContent;
        }
        private void AttachmentToFile()
        {
            if (AttachContent == null)
            {
                FileContent = null;
                return;
            }
            // Формат пока будем определять по первому члену контента вложения. Как делать на самом деле нет документации
            // Для xlsx - это 22, для текстовых файлов - 20. Этот же член определяет длину префикса 
            switch (AttachContent[0])
            {
                case 20:
                    TypeCont = TypeContentEnum.text;
                    break;
                case 22:
                    TypeCont = TypeContentEnum.xlsx;
                    break;
                default: throw (new Exception("Неизвестный формат вложения"));
            }
            FileContent = new byte[AttachContent.Length - AttachContent[0]];
            for (int i = 0; i < (FileContent.Length); i++)
            {
                FileContent[i] = AttachContent[i + AttachContent[0]];
            }
        }
    }

    public enum TypeContentEnum
    {
        text,
        xlsx
    }
}

