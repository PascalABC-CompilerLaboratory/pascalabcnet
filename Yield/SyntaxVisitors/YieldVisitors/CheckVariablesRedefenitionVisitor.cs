using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PascalABCCompiler;
using PascalABCCompiler.SyntaxTree;

namespace SyntaxVisitors
{
    public class CheckVariablesRedefenitionVisitor : WalkingVisitorNew
    {
        private ISet<string> UpperBlockNames { get; set; }

        // Map :: CurrentLevel -> { BlockNames }
        private List<ISet<string>> BlockNamesStack { get;  set; }

        private int CurrentLevel = -1;

        public CheckVariablesRedefenitionVisitor(ISet<string> upperBlockNames)
        {
            this.UpperBlockNames = upperBlockNames;
            this.BlockNamesStack = new List<ISet<string>>(10);
        }


        /*
         * // Scope Level 0
         * begin 
         *   var x = value; // Name in Scope Level 0
         *   // Scope Level 1
         *   begin
         *     var x = value; // Name in Scope Level 1 <-- REDEFENITION ERROR, x defined at Level 0
         *     
         *     var y = value;
         *     var y = value2; // Name in Scope Level 1 <-- REDEFINITION ERROR, y defined at Level 1
         *     
         *     // Scope Level 2
         *     begin
         *       var z = y; // Name in Scope Level 2
         *     end
         *     // Scope Level 2
         *     begin
         *       var z = value; // Name in Scope Level 2
         *     end
         *   end
         * 
         * end
        */

        public override void visit(statement_list stlist)
        {
            ++CurrentLevel;

            if (BlockNamesStack.Count <= CurrentLevel)
            {
                // Создаем множество имен для текущего уровня вложенности мини-пространства имен
                BlockNamesStack.Add(new HashSet<string>());
            }

            

            for (var i = 0; i < stlist.list.Count; ++i)
                ProcessNode(stlist.list[i]);

            BlockNamesStack.RemoveAt(BlockNamesStack.Count - 1);

            --CurrentLevel;
        }

        public override void visit(var_statement vs)
        {
            foreach (var name in vs.var_def.vars.idents.Select(id => id.name))
            {
                // Проверяем есть ли такое имя выше?
                CheckVariableAlreadyDefined(name);

                BlockNamesStack[CurrentLevel].Add(name);
            }

            base.visit(vs);
        }

        public override void visit(variable_definitions vd)
        {
            var varNames = vd.var_definitions.SelectMany(vds => vds.vars.idents.Select(id => id.name));
            foreach (var name in varNames)
            {
                CheckVariableAlreadyDefined(name);
                UpperBlockNames.Add(name);
            }

            base.visit(vd);
        }

        public override void visit(for_node fn)
        {
            if (fn.create_loop_variable)
            {
                CheckVariableAlreadyDefined(fn.loop_variable.name);
            }
            base.visit(fn);
        }

        private bool IsVariableAlreadyDefined(string name)
        {
            if (UpperBlockNames.Contains(name))
                return true;
            for (int i = 0; i <= CurrentLevel; ++i)
            {
                if (BlockNamesStack[i].Contains(name))
                    return true;
            }
            return false;
        }

        private void CheckVariableAlreadyDefined(string name)
        {
            if (IsVariableAlreadyDefined(name))
            {
                throw new Exception(string.Format("Var {0} is already defined", name));
            }
        }
        
    }
}
