using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    class Question
    {
        private const int MaxQnameSize = 255; // rfc 1035

        private Question()
        {

        }

        public String QName { get; set; }
        public RecordType QType { get; set; }
        public RecordClass QClass { get; set; }

        public static Question Parse(byte[] buffer)
        {
            buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

            Question q = new Question();
            //q.QName = ParseQuestionName(buffer, out int bitsRead);
            //byte[] rem = TrimQName(buffer, bitsRead);
            //q.QType = ParseQueryType(buffer, bitsRead);
            //q.QClass = ParseQueryClass(buffer, bitsRead);

            return q;
        }
    }
}
