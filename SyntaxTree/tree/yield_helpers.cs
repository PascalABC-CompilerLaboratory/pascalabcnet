using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PascalABCCompiler.SyntaxTree
{
    ///<summary>
    ///Нераспознанный идентификатор
    ///</summary>
    [Serializable]
    public partial class unknown_ident : addressed_value_funcname
    {
        public ident UnknownID { get; private set; }

        public ident ClassName { get; private set; }

        public unknown_ident(ident _unk, ident className)
        {
            this.UnknownID = _unk;
            this.ClassName = className;
        }

        public unknown_ident(ident _unk, SourceContext sc)
        {
            this.UnknownID = _unk;
            source_context = sc;
        }

        #region Helpers

        ///<summary>
        ///Свойство для получения количества всех подузлов без элементов поля типа List
        ///</summary>
        public override Int32 subnodes_without_list_elements_count
        {
            get
            {
                return 0;
            }
        }
        ///<summary>
        ///Свойство для получения количества всех подузлов. Подузлом также считается каждый элемент поля типа List
        ///</summary>
        public override Int32 subnodes_count
        {
            get
            {
                return 0;
            }
        }

        ///<summary>
        ///Индексатор для получения всех подузлов
        ///</summary>
        public override syntax_tree_node this[Int32 ind]
        {
            get
            {
                if (subnodes_count == 0 || ind < 0 || ind > subnodes_count - 1)
                    throw new IndexOutOfRangeException();
                return null;
            }
            set
            {
                if (subnodes_count == 0 || ind < 0 || ind > subnodes_count - 1)
                    throw new IndexOutOfRangeException();
            }
        }

        ///<summary>
        ///Метод для обхода дерева посетителем
        ///</summary>
        ///<param name="visitor">Объект-посетитель.</param>
        ///<returns>Return value is void</returns>
        public override void visit(IVisitor visitor)
        {
            visitor.visit(this);
        }

        #endregion
    }

    
}
