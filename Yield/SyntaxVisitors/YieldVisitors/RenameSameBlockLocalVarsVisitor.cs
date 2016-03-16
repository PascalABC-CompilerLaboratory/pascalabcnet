using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PascalABCCompiler;
using PascalABCCompiler.SyntaxTree;

namespace SyntaxVisitors
{
    public class RenameSameBlockLocalVarsVisitor : BaseChangeVisitor
    {
        // Уровень вложенности -> (localName -> countAtThisLevel)
        private Dictionary<int, Dictionary<string, int>> BlockLocalVarMapStack = new Dictionary<int, Dictionary<string, int>>();
        private int CurrentLevel = 0;

        public RenameSameBlockLocalVarsVisitor()
        {

        }

        public override void visit(declarations decls)
        {
            // DO NOTHING
        }

        public override void visit(statement_list stlist)
        {
            ++CurrentLevel;

            if (!BlockLocalVarMapStack.ContainsKey(CurrentLevel))
            {
                BlockLocalVarMapStack.Add(CurrentLevel, new Dictionary<string, int>());
            }

            for (var i = 0; i < stlist.list.Count; ++i)
                ProcessNode(stlist.list[i]);

            --CurrentLevel;
        }

        public override void visit(var_statement vs)
        {
            foreach (var varName in vs.var_def.vars.idents.Select(id => id.name))
            {
                // Проверяем есть ли такое имя в statement_list на уровень выше?

                if (BlockLocalVarMapStack[CurrentLevel].ContainsKey(varName))
                {
                    // Уже есть такая переменная - увеличиваем счетчик
                    ++BlockLocalVarMapStack[CurrentLevel][varName];
                }
                else
                {
                    // Нет - добавляем
                    BlockLocalVarMapStack[CurrentLevel].Add(varName, 0);
                }
            }

            var newLocalNames = vs.var_def.vars.idents.Select(id => this.CreateSameLocalVariable(id));

            Replace(vs, new var_statement(new var_def_statement(new ident_list(newLocalNames.ToArray()), vs.var_def.vars_type, vs.var_def.inital_value)));

            base.visit(vs);
        }

        public override void visit(ident id)
        {
            if (BlockLocalVarMapStack[CurrentLevel].ContainsKey(id.name))
            {
                Replace(id, this.CreateSameLocalVariable(id));
            }
        }

        public override void visit(dot_node dn)
        {
        }

        private ident CreateSameLocalVariable(ident id)
        {
            return new ident(id.name + "_" + BlockLocalVarMapStack[CurrentLevel][id.name], id.source_context);
        }
    }
}
