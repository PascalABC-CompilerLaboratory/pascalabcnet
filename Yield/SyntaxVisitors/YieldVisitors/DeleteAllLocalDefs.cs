using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PascalABCCompiler;
using PascalABCCompiler.SyntaxTree;

namespace SyntaxVisitors
{
    public class DeleteAllLocalDefs : BaseChangeVisitor
    {
        public List<var_def_statement> LocalDeletedDefs = new List<var_def_statement>(); // все локальные описания

        public ISet<string> LocalDeletedDefsNames = new HashSet<string>();            // их имена - для быстрого поиска  

        public ISet<ident> CollectedLocals = new HashSet<ident>();

        public DeleteAllLocalDefs() // надо запускать этот визитор начиная с корня подпрограммы
        {
        }

        public override void visit(var_statement vs) // локальные описания внутри процедуры
        {
            LocalDeletedDefs.Add(vs.var_def);
            DeleteInStatementList(vs);

            // frninja 02/03/16 - fix capturing unknown idents -> replace delete with assign
            /*if ((object)vs.var_def.inital_value == null)
            {
                DeleteInStatementList(vs);
            }
            else
            {
                Replace(vs, new statement_list(SeqStatements(vs.var_def.vars.idents.Select(id => new assign(id, vs.var_def.inital_value)).ToArray()).ToArray()));
            }*/

            LocalDeletedDefsNames.UnionWith(vs.var_def.vars.idents.Select(id => id.name));
            CollectedLocals.UnionWith(vs.var_def.vars.idents);
        }

        public override void visit(variable_definitions vd)
        {
            foreach (var v in vd.list)
            {
                LocalDeletedDefs.Add(v);
                LocalDeletedDefsNames.UnionWith(v.vars.idents.Select(id => id.name));
            }
            var d = UpperNodeAs<declarations>();
            d.defs.Remove(vd); // может ли остаться список declarations пустым?

            // frninja 03/03/16 - переносим объявления переменных c initial_value в тело метода для дальнейшей обработки
            var methodBody = UpperTo<block>().program_code;

            methodBody.list.InsertRange(0, vd.list
                    .Where(vds => (object)vds.inital_value != null)
                    .SelectMany(vdsInit => vdsInit.vars.list.Select(id => new assign(id, vdsInit.inital_value))));

            // еще - не заходить в лямбды
        }
    }
}
