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

        public vars_initial_values_type_helper Vars { get; private set; }

        public unknown_expression_type(var_def_statement vds, vars_initial_values_type_helper vars)
        {
            this.Vds = vds;
            this.Vars = vars;
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


    /// <summary>
    /// Bang-fake-узел для yield
    /// </summary>
    [Serializable]
    public class vars_initial_values_type_helper : statement
    {
        public List<var_def_statement> Vars { get; private set; }

        public Dictionary<var_def_statement, object> VarsTypeMap { get; private set; }
        
        public vars_initial_values_type_helper(IEnumerable<var_def_statement> vars)
        {
            this.Vars = new List<var_def_statement>(vars);
            this.VarsTypeMap = new Dictionary<var_def_statement, object>();
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
