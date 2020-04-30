using System;
namespace MS_Access_DB
{
    public class Attachment
    {
        public string Name { get; private set; }
        public TypeContentEnum TypeCont { get; private set; }
        public byte[] AttachContent { get; private set; }
        public byte[] FileContent { get; private set; }
        
        //const int LengthInfo = 22;
        readonly byte[] InfoXml = new byte[20] { 20, 0, 0, 0, 1, 0, 0, 0, 4, 0, 0, 0, 120, 0, 109, 0, 108, 0, 0, 0};
        readonly byte[] InfoXls = new byte[22] { 20, 0, 0, 0, 1, 0, 0, 0, 5, 0, 0, 0, 120, 0, 108, 0, 115, 0, 120, 0, 0, 0};

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
                case TypeContentEnum.txt: prefix = InfoXml;
                    break;
                case TypeContentEnum.xlsx: prefix = InfoXls;
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
            // Для xlsx - это 22, для txt - 20. Как txt сможем записывать xml, json и log Этот же член определяет длину префикса 
            switch (AttachContent[0])
            {
                case 20: TypeCont = TypeContentEnum.txt;
                    break;
                case 22: TypeCont = TypeContentEnum.xlsx;
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
        txt,
        xlsx
    }
}
