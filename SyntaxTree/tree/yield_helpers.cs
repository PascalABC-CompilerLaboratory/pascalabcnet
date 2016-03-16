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

    /// <summary>
    /// Узел для вычисления типа выражения используемого в теле функции-итератора (с yield)
    /// </summary>
    [Serializable]
    public class unknown_expression_type : type_definition
    {
        public var_def_statement Vds { get; private set; }

        public locals_type_map_helper MapHelper { get; private set; }

        public unknown_expression_type(var_def_statement vds, locals_type_map_helper map_helper)
        {
            this.Vds = vds;
            this.MapHelper = map_helper;
        }

        public unknown_expression_type(var_def_statement vds, SourceContext sc)
        {
            this.Vds = vds;
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
 
    // Узел-обертка для yield для определения типов локальных переменных в var_def_statement
    [Serializable]
    public class var_def_statement_with_unknown_type : statement
    {
        public var_def_statement vars { get; private set; }
        public locals_type_map_helper map_helper { get; private set; }

        public var_def_statement_with_unknown_type(var_def_statement vds, locals_type_map_helper map_helper)
        {
            this.vars = vds;
            this.map_helper = map_helper;
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

    // Узел-обертка для yield для определения типов локальных переменных в variable_definitions
    [Serializable]
    public class variable_definitions_with_unknown_type : declaration
    {
        public variable_definitions vars { get; private set; }

        public locals_type_map_helper map_helper { get; private set; }

        public variable_definitions_with_unknown_type(variable_definitions vd, locals_type_map_helper map_helper)
        {
            this.vars = vd;
            this.map_helper = map_helper;
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

    [Serializable]
    public class locals_type_map_helper
    {
        public Dictionary<var_def_statement, semantic_type_node> vars_type_map { get; private set; }

        public locals_type_map_helper()
        {
            vars_type_map = new Dictionary<var_def_statement, semantic_type_node>();
        }
    }
}
