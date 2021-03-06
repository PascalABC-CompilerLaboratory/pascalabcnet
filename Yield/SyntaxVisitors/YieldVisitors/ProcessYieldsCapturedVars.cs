﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PascalABCCompiler;
using PascalABCCompiler.SyntaxTree;

using PascalABCCompiler.ParserTools;
using PascalABCCompiler.Errors;

using PascalABCCompiler.YieldHelpers;

namespace SyntaxVisitors
{

    public static class CapturedNamesHelper
    {
        public static int CurrentLocalVariableNum = 0;

        public static string MakeCapturedFormalParameterName(string formalParamName)
        {
            return string.Format("<>{0}__{1}", YieldConsts.ReservedNum.MethodFormalParam, formalParamName);
        }

        public static string MakeCapturedLocalName(string localName)
        {
            return string.Format("<{0}>{1}__{2}", localName, YieldConsts.ReservedNum.MethodLocalVariable, ++CurrentLocalVariableNum);
        }
    }

    public class ProcessYieldCapturedVarsVisitor : BaseChangeVisitor
    {
        int clnum = 0;

        public string NewYieldClassName()
        {
            clnum++;
            return "clyield#" + clnum.ToString();
        }

        public FindMainIdentsVisitor mids; // захваченные переменные процедуры по всем её yield 

        public int countNodesVisited;

        public bool hasYields = false;

        public static ProcessYieldCapturedVarsVisitor New
        {
            get { return new ProcessYieldCapturedVarsVisitor(); }
        }

        public ProcessYieldCapturedVarsVisitor()
        {
            //PrintInfo = false; 
        }

        public override void Enter(syntax_tree_node st)
        {
            base.Enter(st);
            countNodesVisited++;

            // сокращение обходимых узлов. Как сделать фильтр по тем узлам, которые необходимо обходить? Например, все операторы (без выражений и описаний), все описания (без операторов)
            if (st is assign || st is var_def_statement || st is procedure_call || st is procedure_header || st is expression)
            {
                visitNode = false; // фильтр - куда не заходить 
            }
        }

        /*public override void visit(class_members cm)
        {
            foreach (var decl in cm.members)
            {
                if (decl is procedure_header || decl is procedure_definition)
                    decl.visit(this);
            }
            base.visit(cm);
        }*/

        type_declarations GenClassesForYield(procedure_definition pd,
            IEnumerable<var_def_statement> fields, // локальные переменные
            IDictionary<string, string> localsMap, // отображение для захваченных имен локальных переменных
            IDictionary<string, string> formalParamsMap, // отображение для захваченных имен формальных параметров
            IDictionary<var_def_statement, var_def_statement> localsCloneMap, // отображение для оберток локальных переменных 
            yield_locals_type_map_helper localTypeMapHelper) // вспомогательный узел для типов локальных переменных
        {
            var fh = (pd.proc_header as function_header);
            if (fh == null)
                throw new SyntaxError("Only functions can contain yields", "", pd.proc_header.source_context, pd.proc_header);
            var seqt = fh.return_type as sequence_type;
            if (seqt == null)
                throw new SyntaxError("Functions with yields must return sequences", "", fh.return_type.source_context, fh.return_type);

            // Теперь на месте функции генерируем класс

            // Захваченные локальные переменные
            var cm = class_members.Public;
            var capturedFields = fields.Select(vds =>
                                    {
                                        ident_list ids = new ident_list(vds.vars.idents.Select(id => new ident(localsMap[id.name])).ToArray());
                                        if (vds.vars_type == null) //&& vds.inital_value != null)
                                        {
                                            if (vds.inital_value != null)
                                            {
                                                //return new var_def_statement(ids, new yield_unknown_expression_type(localsCloneMap[vds], varsTypeDetectorHelper), null);
                                                return new var_def_statement(ids, new yield_unknown_expression_type(localsCloneMap[vds], localTypeMapHelper), null);
                                            }
                                            else
                                            {
                                                throw new Exception("Variable defenition without type and value!");
                                            }
                                        }
                                        else
                                        {
                                            return new var_def_statement(ids, vds.vars_type, null);
                                        }
                                        
                                        //return new var_def_statement(ids, vds.vars_type, vds.inital_value);
                                    });

            foreach (var m in capturedFields)
                cm.Add(m);

            // Параметры функции
            List<ident> lid = new List<ident>();
            var pars = fh.parameters;
            if (pars != null)
                foreach (var ps in pars.params_list)
                {
                    if (ps.param_kind != parametr_kind.none)
                        throw new SyntaxError("Parameters of functions with yields must not have 'var', 'const' or 'params' modifier", "", pars.source_context, pars);
                    if (ps.inital_value != null)
                        throw new SyntaxError("Parameters of functions with yields must not have initial values", "", pars.source_context, pars);
                    //var_def_statement vds = new var_def_statement(ps.idents, ps.vars_type);
                    ident_list ids = new ident_list(ps.idents.list.Select(id => new ident(formalParamsMap[id.name])).ToArray());
                    var_def_statement vds = new var_def_statement(ids, ps.vars_type);
                    cm.Add(vds); // все параметры функции делаем полями класса
                    //lid.AddRange(vds.vars.idents);
                    lid.AddRange(ps.idents.list);
                }

            var stels = seqt.elements_type;

            var iteratorClassName = GetClassName(pd);

            // frninja 08/18/15 - Для захвата self
            if (iteratorClassName != null)
            {
                // frninja 20/04/16 - поддержка шаблонных классов
                var iteratorClassRef = CreateClassReference(iteratorClassName);

                cm.Add(new var_def_statement(YieldConsts.Self, iteratorClassRef));
            }

            // Системные поля и методы для реализации интерфейса IEnumerable
            cm.Add(new var_def_statement(YieldConsts.State, "integer"),
                new var_def_statement(YieldConsts.Current, stels),
                procedure_definition.EmptyDefaultConstructor,
                new procedure_definition("Reset"),
                new procedure_definition("MoveNext", "boolean", pd.proc_body),
                new procedure_definition("System.Collections.IEnumerator.get_Current", "object", new assign("Result", YieldConsts.Current)),
                new procedure_definition("System.Collections.IEnumerable.GetEnumerator", "System.Collections.IEnumerator", new assign("Result", "Self"))
                );

            
            

            // frninja 20/04/16 - поддержка шаблонных классов
            var yieldClassName = NewYieldClassName();
            var yieldClassHelperName = yieldClassName + "Helper";

            var className = this.CreateHelperClassName(yieldClassName, iteratorClassName, pd);
            var classNameHelper = this.CreateHelperClassName(yieldClassHelperName, iteratorClassName, pd);
            

            var interfaces = new named_type_reference_list("System.Collections.IEnumerator", "System.Collections.IEnumerable");

            // frninja 24/04/16 - поддержка шаблонных классов
            //var td = new type_declaration(classNameHelper, this.CreateHelperClassDefinition(classNameHelper, pd, interfaces, cm));
                //SyntaxTreeBuilder.BuildClassDefinition(interfaces, cm));

            // Изменение тела процедуры
            

            // frninja 20/04/16 - поддержка шаблонных классов
            var stl = new statement_list(new var_statement("res", new new_expr(this.CreateClassReference(className), new expression_list())));
            

            //stl.AddMany(lid.Select(id => new assign(new dot_node("res", id), id)));
            stl.AddMany(lid.Select(id => new assign(new dot_node("res", new ident(formalParamsMap[id.name])), id)));

            // frninja 08/12/15 - захват self
            if (iteratorClassName != null)
            {
                stl.Add(new assign(new dot_node("res", YieldConsts.Self), new ident("self")));
            }

            stl.Add(new assign("Result", "res"));

            // New body
            pd.proc_body = new block(stl);

            if (iteratorClassName != null)
            {
                var cd = UpperTo<class_definition>();
                if (cd != null)
                {
                    // Если метод описан в классе 
                    // frninja 10/12/15 - заменить на function_header и перенести описание тела в declarations
                    Replace(pd, fh);
                    var decls = UpperTo<declarations>();
                    if (decls != null)
                    {
                        // frninja 12/05/16 - забыли копировать return
                        function_header nfh = ObjectCopier.Clone(fh);
                        //function_header nfh = new function_header();
                        //nfh.name = new method_name(fh.name.meth_name.name);

                        // Set className
                        nfh.name.class_name = iteratorClassName;
                        //nfh.parameters = fh.parameters;
                        //nfh.proc_attributes = fh.proc_attributes;
                        //nfh.return_type = fh.return_type;

                        procedure_definition npd = new procedure_definition(nfh, new block(stl));

                        // Update header
                        //pd.proc_header.className.class_name = GetClassName(pd);
                        // Add to decls
                        decls.Add(npd);
                    }
                }
            }

            // Второй класс

            var tpl = new template_param_list(stels);

            var IEnumeratorT = new template_type_reference("System.Collections.Generic.IEnumerator", tpl);

            var cm1 = cm.Add( //class_members.Public.Add(
                //procedure_definition.EmptyDefaultConstructor,
                new procedure_definition(new function_header("get_Current", stels), new assign("Result", YieldConsts.Current)),
                new procedure_definition(new function_header("GetEnumerator", IEnumeratorT), new assign("Result", "Self")),
                new procedure_definition("Dispose")
            );


            // frninja 20/04/16 - поддержка шаблонных классов
            var interfaces1 = new named_type_reference_list(/*this.CreateClassReference(classNameHelper) as named_type_reference*/);
            var IEnumerableT = new template_type_reference("System.Collections.Generic.IEnumerable", tpl);

            interfaces1.Add(IEnumerableT).Add(IEnumeratorT);

            // frninja 24/04/16 - поддержка шаблонных классов
            var td1 = new type_declaration(className, this.CreateHelperClassDefinition(className, pd, interfaces1, cm1));
                //SyntaxTreeBuilder.BuildClassDefinition(interfaces1, cm1));

            var cct = new type_declarations(/*td*/);
            cct.Add(td1);

            return cct;
        }

        private void CollectFormalParams(procedure_definition pd, ISet<var_def_statement> collectedFormalParams)
        {
            if (pd.proc_header.parameters != null)
                collectedFormalParams.UnionWith(pd.proc_header.parameters.params_list.Select(tp => new var_def_statement(tp.idents, tp.vars_type)));
        }

        private void CollectFormalParamsNames(procedure_definition pd, ISet<string> collectedFormalParamsNames)
        {
            if (pd.proc_header.parameters != null)
                collectedFormalParamsNames.UnionWith(pd.proc_header.parameters.params_list.SelectMany(tp => tp.idents.idents).Select(id => id.name));
        }


        /// <summary>
        /// Создает обращение к имени класса по имени класса
        /// </summary>
        /// <param name="className">Имя класса</param>
        /// <returns></returns>
        private type_definition CreateClassReference(ident className)
        {
            if (className is template_type_name)
            {
                return new template_type_reference(
                    new named_type_reference(className),
                    new template_param_list(string.Join(",", (className as template_type_name).template_args.idents.Select(id => id.name)))
                    );
            }
            return new named_type_reference(className);
        }


        /// <summary>
        /// Создает имя вспомогательного класса
        /// </summary>
        /// <param name="helperName">Имя вспомогательного класса</param>
        /// <param name="className">Имя класса</param>
        /// <returns></returns>
        private ident CreateHelperClassName(string helperName, ident className, procedure_definition pd)
        {
            if (className is template_type_name)
            {
                return new template_type_name(helperName, (className as template_type_name).template_args);
            }
            else if (pd.proc_header.template_args != null)
            {
                return new template_type_name(helperName, pd.proc_header.template_args);
            }
            return new ident(helperName);
        }

        private class_definition CreateHelperClassDefinition(ident className, procedure_definition pd, named_type_reference_list parents, params class_members[] cms)
        {
            if (className is template_type_name)
            {
                return SyntaxTreeBuilder.BuildClassDefinition(parents, (className as template_type_name).template_args , cms);
            }
            else if (pd.proc_header.template_args != null)
            {
                return SyntaxTreeBuilder.BuildClassDefinition(parents, pd.proc_header.template_args, cms);
            }
            return SyntaxTreeBuilder.BuildClassDefinition(parents, cms);
        }


        /// <summary>
        /// Получает имя класса, в котором описан метод-итератор
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        private ident GetClassName(procedure_definition pd)
        {
            if (pd.proc_header.name.class_name != null)
            {
                // Объявление вне класса его метода
                return pd.proc_header.name.class_name;
            }
            else
            {
                // Объявление функции в классе?
                var classDef = UpperNode(3) as class_definition;
                if ((UpperNode(3) as class_definition) != null)
                {
                    var td = UpperNode(4) as type_declaration;
                    if (td != null)
                    {
                        return td.type_name;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Определяет описан ли метод-итератор в некотором классе
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        private bool IsClassMethod(procedure_definition pd)
        {
            return GetClassName(pd) != null;
        }

        private void CollectClassFieldsNames(procedure_definition pd, ISet<string> collectedFields)
        {
            ident className = GetClassName(pd);

            if (className != null)
            {
                CollectClassFieldsVisitor fieldsVis = new CollectClassFieldsVisitor(className);
                var cu = UpperTo<compilation_unit>();
                if (cu != null)
                {
                    cu.visit(fieldsVis);
                    // Collect
                    collectedFields.UnionWith(fieldsVis.CollectedFields.Select(id => id.name));
                }
            }
        }

        private void CollectClassMethodsNames(procedure_definition pd, ISet<string> collectedMethods)
        {
            ident className = GetClassName(pd);

            if (className != null)
            {
                CollectClassMethodsVisitor methodsVis = new CollectClassMethodsVisitor(className);
                var cu = UpperTo<compilation_unit>();
                if (cu != null)
                {
                    cu.visit(methodsVis);
                    // Collect
                    collectedMethods.UnionWith(methodsVis.CollectedMethods.Select(id => id.name));
                }
            }
        }

        private void CollectClassPropertiesNames(procedure_definition pd, ISet<string> collectedProperties)
        {
            ident className = GetClassName(pd);

            if (className != null)
            {
                CollectClassPropertiesVisitor propertiesVis = new CollectClassPropertiesVisitor(className);
                var cu = UpperTo<compilation_unit>();
                if (cu != null)
                {
                    cu.visit(propertiesVis);
                    // Collect
                    collectedProperties.UnionWith(propertiesVis.CollectedProperties.Select(id => id.name));
                }
            }
        }

        private void CollectUnitGlobalsNames(procedure_definition pd, ISet<string> collectedUnitGlobalsName)
        {
            var cu = UpperTo<compilation_unit>();
            if (cu != null)
            {
                var ugVis = new CollectUnitGlobalsVisitor();
                cu.visit(ugVis);
                // Collect
                collectedUnitGlobalsName.UnionWith(ugVis.CollectedGlobals.Select(id => id.name));
            }
        }

        private void CreateCapturedLocalsNamesMap(ISet<string> localsNames, IDictionary<string, string> capturedLocalsNamesMap)
        {
            foreach (var localName in localsNames)
            {
                capturedLocalsNamesMap.Add(localName, CapturedNamesHelper.MakeCapturedLocalName(localName));
            }
        }

        private void CreateCapturedFormalParamsNamesMap(ISet<string> formalParamsNames, IDictionary<string, string> captueedFormalParamsNamesMap)
        {
            foreach (var formalParamName in formalParamsNames)
            {
                captueedFormalParamsNamesMap.Add(formalParamName, CapturedNamesHelper.MakeCapturedFormalParameterName(formalParamName));
            }
        }

        /// <summary>
        /// Обработка локальных переменных метода и их типов для корректного захвата
        /// </summary>
        /// <param className="pd">Объявление метода</param>
        /// <returns>Коллекция посещенных локальных переменных</returns>
        private void CreateLocalVariablesTypeProxies(procedure_definition pd, out IEnumerable<var_def_statement> localsClonesCollection, out yield_locals_type_map_helper localsTypeMapHelper)
        {
            // Выполняем определение типов локальных переменных с автовыводом типов

            // Клонируем исходный метод для вставки оберток-хелперов для локальных переменных и дальнейшей обработки на семантике
            var pdCloned = ObjectCopier.Clone(pd);

            // Заменяем локальные переменные с неизвестным типом на обертки-хелперы (откладываем до семантики)
            localsTypeMapHelper = new yield_locals_type_map_helper();
            LocalVariablesTypeDetectorHelperVisior localsTypeDetectorHelperVisitor = new LocalVariablesTypeDetectorHelperVisior(localsTypeMapHelper);
            pdCloned.visit(localsTypeDetectorHelperVisitor);

            // frninja 16/03/16 - строим список локальных переменных в правильном порядке
            localsTypeDetectorHelperVisitor.LocalDeletedDefs.AddRange(localsTypeDetectorHelperVisitor.LocalDeletedVS);
            localsTypeDetectorHelperVisitor.LocalDeletedVS.Clear();

            localsClonesCollection = localsTypeDetectorHelperVisitor.LocalDeletedDefs.ToArray();

            // Добавляем в класс метод с обертками для локальных переменных
            pdCloned.proc_header.name.meth_name = new ident(YieldConsts.YieldHelperMethodPrefix+ "_locals_type_detector>" + pd.proc_header.name.meth_name.name); // = new method_name("<yield_helper_locals_type_detector>" + pd.proc_header.className.meth_name.className);
            if (IsClassMethod(pd))
            {
                var cd = UpperTo<class_definition>();
                if (cd != null)
                {
                    // Метод класса описан в классе
                    var classMembers = UpperTo<class_members>();
                    classMembers.Add(pdCloned);
                }
                else
                {
                    // Метод класса описан вне класса

                    var decls = UpperTo<declarations>();
                    var classMembers = decls.list
                        .Select(decl => decl as type_declarations)
                        .Where(tdecls => tdecls != null)
                        .SelectMany(tdecls => tdecls.types_decl)
                        .Where(td => td.type_name.name == GetClassName(pd).name)
                        .Select(td => td.type_def as class_definition)
                        .Where(_cd => _cd != null)
                        .SelectMany(_cd => _cd.body.class_def_blocks);


                    // Вставляем предописание метода-хелпера 
                    var helperPredefHeader = ObjectCopier.Clone(pdCloned.proc_header);
                    helperPredefHeader.name.class_name = null;
                    classMembers.First().members.Insert(0, helperPredefHeader);

                    // Вставляем тело метода-хелпера
                    UpperTo<declarations>().InsertBefore(pd, pdCloned);
                }
            }
            else
            {
                UpperTo<declarations>().InsertBefore(pd, pdCloned);
            }
        }

        /// <summary>
        /// Отображение локальных в клонированные локальные
        /// </summary>
        /// <param className="from">Откуда</param>
        /// <param className="to">Куда</param>
        /// <returns>Отображение</returns>
        private Dictionary<var_def_statement, var_def_statement> CreateLocalsClonesMap(IEnumerable<var_def_statement> from, IEnumerable<var_def_statement> to)
        {
            // Нужно тк клонировали метод для создания хелпера-определителя типов локальных переменных - Eq не будет работать

            // Строим отображение из локальных переменных клона оригинального метода в локальные переменные основного метода
            Dictionary<var_def_statement, var_def_statement> localsClonesMap = new Dictionary<var_def_statement, var_def_statement>();

            var localsArr = from.ToArray();
            var localsClonesArr = to.ToArray();

            // Create map :: locals -> cloned locals
            for (int i = 0; i < localsArr.Length; ++i)
            {
                localsClonesMap.Add(localsArr[i], localsClonesArr[i]);
            }

            return localsClonesMap;
        }

        /// <summary>
        /// Вставляем описание классов-хелперов для yield перед методом-итератором в зависимости от его описания
        /// </summary>
        /// <param className="pd">Метод-итератор</param>
        /// <param className="cct">Описание классов-хелперов для yield</param>
        private void InsertYieldHelpers(procedure_definition pd, type_declarations cct)
        {
            if (IsClassMethod(pd))
            {
                var cd = UpperTo<class_definition>();
                if (cd != null)
                {
                    // Если метод класса описан в классе
                    var td = UpperTo<type_declarations>();

                    // frninja 20/04/16 - выпилено 

                    //foreach (var helperName in cct.types_decl.Select(ttd => ttd.type_name))
                    //{
                    //    var helperPredef = new type_declaration(helperName, new class_definition());
                        //td.types_decl.Insert(0, helperPredef);
                    //}

                    // Insert class predefenition!
                    //var iteratorClassPredef = new type_declaration(GetClassName(pd), new class_definition(null));
                    //td.types_decl.Insert(0, iteratorClassPredef);

                    foreach (var helper in cct.types_decl)
                    {
                        td.types_decl.Add(helper);
                    }

                }
                else
                {
                    // Метод класса описан вне класса
                    UpperTo<declarations>().InsertBefore(pd, cct);
                }
            }
            else
            {
                UpperTo<declarations>().InsertBefore(pd, cct);
            }
        }

        /// <summary>
        /// Захватываем имена в методе
        /// </summary>
        /// <param className="pd">Метод-итератор</param>
        /// <param className="deletedLocals">Коллекция удаленных локальных переменных</param>
        /// <param className="capturedLocalsNamesMap">Построенное отображение имен локальных переменных в захваченные имена</param>
        /// <param className="capturedFormalParamsNamesMap">Построенное отображение имен формальных параметров в захваченные имена</param>
        private void ReplaceCapturedVariables(procedure_definition pd,
            IEnumerable<var_def_statement> deletedLocals,
            out IDictionary<string, string> capturedLocalsNamesMap,
            out IDictionary<string, string> capturedFormalParamsNamesMap)
        {
            // Структуры данных под классификацию имен в методе

            // Classification
            ISet<string> CollectedLocalsNames = new HashSet<string>();
            ISet<string> CollectedFormalParamsNames = new HashSet<string>();
            ISet<string> CollectedClassFieldsNames = new HashSet<string>();
            ISet<string> CollectedClassMethodsNames = new HashSet<string>();
            ISet<string> CollectedClassPropertiesNames = new HashSet<string>();
            ISet<string> CollectedUnitGlobalsNames = new HashSet<string>();

            ISet<var_def_statement> CollectedLocals = new HashSet<var_def_statement>();
            ISet<var_def_statement> CollectedFormalParams = new HashSet<var_def_statement>();

            // Map from ident idName -> captured ident idName
            capturedLocalsNamesMap = new Dictionary<string, string>();
            capturedFormalParamsNamesMap = new Dictionary<string, string>();

            // Собираем инфу о именах

            // Collect locals
            CollectedLocals.UnionWith(deletedLocals);
            CollectedLocalsNames.UnionWith(deletedLocals.SelectMany(vds => vds.vars.idents).Select(id => id.name));
            // Collect formal params
            CollectFormalParams(pd, CollectedFormalParams);
            CollectFormalParamsNames(pd, CollectedFormalParamsNames);
            // Collect class fields
            CollectClassFieldsNames(pd, CollectedClassFieldsNames);
            // Collect class methods
            CollectClassMethodsNames(pd, CollectedClassMethodsNames);
            // Collect class properties
            CollectClassPropertiesNames(pd, CollectedClassPropertiesNames);
            // Collect unit globals
            CollectUnitGlobalsNames(pd, CollectedUnitGlobalsNames);

            // Строим отображения для имён захваченных локальных переменных и формальных параметров

            // Create maps :: idName -> captureName
            CreateCapturedLocalsNamesMap(CollectedLocalsNames, capturedLocalsNamesMap);
            CreateCapturedFormalParamsNamesMap(CollectedFormalParamsNames, capturedFormalParamsNamesMap);

            // Выполняем замену захват имён в теле метода
            // AHAHA test!
            ReplaceCapturedVariablesVisitor rcapVis = new ReplaceCapturedVariablesVisitor(
                CollectedLocalsNames,
                CollectedFormalParamsNames,
                CollectedClassFieldsNames,
                CollectedClassMethodsNames,
                CollectedClassPropertiesNames,
                CollectedUnitGlobalsNames,
                capturedLocalsNamesMap,
                capturedFormalParamsNamesMap,
                IsClassMethod(pd),
                GetClassName(pd)
                );
            // Replace
            (pd.proc_body as block).program_code.visit(rcapVis);
        }

        private bool IsExtensionMethod(procedure_definition pd)
        {
            var tdecls = UpperTo<declarations>().defs.OfType<type_declarations>().SelectMany(tds => tds.types_decl);

            var isExtension = pd.proc_header.proc_attributes.proc_attributes.Any(attr => attr.name == "extensionmethod");

            if (isExtension)
            {
                // Метод объявлен как extensionmethod
                
                // !!!!!!!! TODO: Проверить что имя класса не находится в этом модуле.

                // Убираем за ненадобностью имя класса ибо оно указано как тип обязательного параметра self
                
                pd.proc_header.name.class_name = null;
                return true;
            }
            else
            {
                // Если не похоже на метод-расширение или объявление вне класса
                if (pd.proc_header.name.class_name == null)
                    return false;

                
                // Разрешаем только имена типов из этого модуля (не расширения)
                if (!tdecls.Any(td => td.type_name.name == pd.proc_header.name.class_name.name))
                {
                    // Имя в модуле не найдено -> метод расширение описанный без extensionmethod. Ругаемся!!!
                    throw new SyntaxError("Possible extension-method definintion without extensionmethod keyword. Please use extensionmethod syntax",
                        "",
                        pd.proc_header.source_context,
                        pd.proc_header);
                }
                
            }
            
            return false;
        }

        public override void visit(procedure_definition pd)
        {
            if (pd.proc_header.name.meth_name.name.StartsWith(YieldConsts.YieldHelperMethodPrefix))
                return;

            //var isExtension = IsExtensionMethod(pd);

            hasYields = false;
            if (pd.proc_header is function_header)
                mids = new FindMainIdentsVisitor();

            base.visit(pd);

            if (!hasYields) // т.е. мы разобрали функцию и уже выходим. Это значит, что пока yield будет обрабатываться только в функциях. Так это и надо.
                return;

            // Проверяем проблемы имен для for
            CheckVariablesRedefenitionVisitor checkVarRedefVisitor = new CheckVariablesRedefenitionVisitor(
                new HashSet<string>(
                    pd.proc_header.parameters != null
                    ?
                    pd.proc_header.parameters.params_list.SelectMany(fp => fp.idents.idents.Select(id => id.name))
                    :
                    Enumerable.Empty<string>()));
            pd.visit(checkVarRedefVisitor);

            // Переименовываем одинаковые имена в мини-ПИ
            RenameSameBlockLocalVarsVisitor renameLocalsVisitor = new RenameSameBlockLocalVarsVisitor();
            pd.visit(renameLocalsVisitor);

            // Теперь lowering
            LoweringVisitor.Accept(pd);

            // frninja 13/04/16 - убираем лишние begin..end
            DeleteRedundantBeginEnds deleteBeginEndVisitor = new DeleteRedundantBeginEnds();
            pd.visit(deleteBeginEndVisitor);

            // Обработка метода для корректного захвата локальных переменных и их типов
            yield_locals_type_map_helper localsTypeMapHelper;
            IEnumerable<var_def_statement> localsClonesCollection;
            CreateLocalVariablesTypeProxies(pd, out localsClonesCollection, out localsTypeMapHelper);
            

            // frninja 16/11/15: перенес ниже чтобы работал захват для lowered for

            var dld = new DeleteAllLocalDefs(); // mids.vars - все захваченные переменные
            pd.visit(dld); // Удалить в локальных и блочных описаниях этой процедуры все переменные и вынести их в отдельный список var_def_statement

            // Строим отображение из локальных переменных клона оригинального метода в локальные переменные основного метода
            Dictionary<var_def_statement, var_def_statement> localsCloneMap = CreateLocalsClonesMap(dld.LocalDeletedDefs, localsClonesCollection);

            // frninja 08/12/15

            // Выполняем захват имён
            IDictionary<string, string> CapturedLocalsNamesMap;
            IDictionary<string, string> CapturedFormalParamsNamesMap;
            ReplaceCapturedVariables(pd, dld.LocalDeletedDefs, out CapturedLocalsNamesMap, out CapturedFormalParamsNamesMap);


            mids.vars.Except(dld.LocalDeletedDefsNames); // параметры остались. Их тоже надо исключать - они и так будут обработаны
            // В результате работы в mids.vars что-то осталось. Это не локальные переменные и с ними непонятно что делать

            // Обработать параметры! 
            // Как? Ищем в mids formal_parametrs, но надо выделить именно обращение к параметрам - не полям класса, не глобальным переменным

            var cfa = new ConstructFiniteAutomata((pd.proc_body as block).program_code);
            cfa.Transform();

            (pd.proc_body as block).program_code = cfa.res;

            // Конструируем определение класса
            var cct = GenClassesForYield(pd, dld.LocalDeletedDefs, CapturedLocalsNamesMap, CapturedFormalParamsNamesMap, localsCloneMap, localsTypeMapHelper); // все удаленные описания переменных делаем описанием класса

            // Вставляем классы-хелперы
            InsertYieldHelpers(pd, cct);

            

            mids = null; // вдруг мы выйдем из процедуры, не зайдем в другую, а там - оператор! Такого конечно не может быть
        }

        public override void visit(yield_node yn)
        {
            hasYields = true;
            if (mids != null) // если мы - внутри процедуры
                yn.visit(mids);
            else throw new SyntaxError("Yield must be in functions only", "", yn.source_context, yn);
            // mids.vars - надо установить, какие из них - локальные, какие - из этого класса, какие - являются параметрами функции, а какие - глобальные (все остальные)
            // те, которые являются параметрами, надо скопировать в локальные переменные и переименовать использование везде по ходу данной функции 
            // самое сложное - переменные-поля этого класса - они требуют в создаваемом классе, реализующем итератор, хранить Self текущего класса и добавлять это Self везде по ходу алгоритма
            // вначале будем считать, что переменные-поля этого класса и переменные-параметры не захватываются yield
            //base.visit(yn);


        }
    }

    class ConstructFiniteAutomata
    {
        public statement_list res = new statement_list();
        statement_list stl;
        int curState = 0;

        statement_list curStatList;
        statement_list StatListAfterCase = new statement_list();

        case_node cas; // формируемый case

        //private Dictionary<labeled_statement, List<int>> dispatches = new Dictionary<labeled_statement, List<int>>();

        private labeled_statement OuterLabeledStatement;
        private Dictionary<int, labeled_statement> Dispatches = new Dictionary<int,labeled_statement>();

        public ConstructFiniteAutomata(statement_list stl)
        {
            this.stl = stl;
        }

        private void AddState(out int stateNumber, out ident resumeLabel)
        {
            stateNumber = curState++;
            resumeLabel = null;
        }

        public void Process(statement st)
        {
            if (!(st is yield_node || st is labeled_statement))
            {
                curStatList.Add(st);
            }
            if (st is yield_node)
            {

                var yn = st as yield_node;
                curState += 1;
                curStatList.AddMany(
                    new assign(YieldConsts.Current, yn.ex),
                    new assign(YieldConsts.State, curState),
                    new assign("Result", true),
                    new procedure_call("exit")
                );

                curStatList = new statement_list();
                
                case_variant cv = new case_variant(new expression_list(new int32_const(curState)), curStatList);
                cas.conditions.variants.Add(cv);
            }
            if (st is labeled_statement)
            {
                var ls = st as labeled_statement;

                // frninja 13/04/16 - диспетчерезация к следующему состоянию
                curStatList.Add(new goto_statement(ls.label_name));

                curStatList = StatListAfterCase;
                curStatList.Add(new labeled_statement(ls.label_name));
                Process(ls.to_statement);

            }
        }

        public void Transform()
        {
            cas = new case_node(new ident(YieldConsts.State));

            curStatList = new statement_list();
            case_variant cv = new case_variant(new expression_list(new int32_const(curState)), curStatList);
            cas.conditions.variants.Add(cv);

            foreach (var st in stl.subnodes)
                Process(st);

            

            // frninja 13/04/16 - фикс для зависающего в последнем состоянии
            var lastStateCV = cas.conditions.variants.Last().exec_if_true as statement_list;
            if (lastStateCV != null)
            {
                lastStateCV.Add(new procedure_call("exit"));
            }

            stl.subnodes = BaseChangeVisitor.SeqStatements(cas, StatListAfterCase).ToList();
            //statement_list res = new statement_list(cas);
            res = stl;
        }
    }

}
