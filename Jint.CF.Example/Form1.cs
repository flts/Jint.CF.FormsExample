using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Jint.Parser;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Jint.Native;
using System.Collections;

namespace Jint.Example
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// TODO 
        /// - test parsing of Js lib file with default functions
        /// - test invoking those functions
        /// - test with execution of those functions
        /// - test CLR importNamespace
        /// - tested CLR function
        /// - tested CLR Object
        /// - tested CLR eventhandler
        /// - tested CLR dictionary
        /// - tested CLR enum
        /// </summary>
        private string _basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase); //.Substring(0, Assembly.GetExecutingAssembly().GetName().CodeBase.LastIndexOf('\\'));
        internal Engine _engine;

        public Form1()
        {
            InitializeComponent();

            _engine = new Engine(o => o.AllowClr(typeof(Form1).Assembly, typeof(System.Windows.Forms.Button).Assembly, typeof(System.Drawing.Point).Assembly)
                                       .AllowDebuggerStatement(true))
                .SetValue("log", new Action<object>(x => Debug.WriteLine(x)))
                .SetValue("mainForm", this)
                .SetValue("msgBoxOk", new Func<string, string, DialogResult>((title, message) => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1)))
                .SetValue("msgBoxYesNo", new Func<string, string, DialogResult>((title, message) => MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1)))
                .SetValue("clearEventInvocations", new Action<object, string>((object obj, string eventName) =>
                {
                    FieldInfo field = null;
                    Type type = obj.GetType();
                    while (type != null)
                    {
                        /* Find events defined as field */
                        field = type.GetField(eventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                        if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                            break;

                        /* Find events defined as property { add; remove; } */
                        field = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                        if (field != null)
                            break;
                        type = type.BaseType;
                    }
                    if (field == null) return;
                    field.SetValue(obj, null);
                }))
                ;

            _engine.Global.FastAddProperty("Jint", new Jint.Runtime.Interop.NamespaceReference(_engine, "Jint"), false, false, false);

            LoadJavascriptFile(_basePath + "\\Scripts\\" + "base.js");
            LoadJavascriptFile(_basePath + "\\Scripts\\" + "CompactJS.js"); ;
        }

        private void LoadJavascriptFile(string scriptPath)
        {
            try
            {
                if (File.Exists(scriptPath))
                {
                    using (var stream = new StreamReader(scriptPath, System.Text.Encoding.UTF8))
                    {
                        var source = stream.ReadToEnd();
                        RunJavascript(source);
                    }
                }
                else
                {
                    Debug.WriteLine(string.Format("File '{0}' does not exists!", scriptPath));
                }
            }
            catch (ParserException px)
            {
                var i = px;
            }
            catch (JavaScriptException jx)
            {
                var i = jx;
            }
            catch (Exception ex)
            {
                var i = ex;
            }
        }

        private void RunJavascript(string source)
        {
            try
            {
                _engine.Execute(source);
            }
            catch (ParserException px)
            {
                var i = px;
            }
            catch (JavaScriptException jx)
            {
                var i = jx;
            }
            catch (Exception ex)
            {
                var i = ex;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RunJavascript(@"CompactJS.Application.changeAppTitle('" + textBox1.Text + "');");

            //_engine.Invoke("CompactJS.Application.changeAppTitle", textBox1.Text); // cannot call nested functions ....
        }

        ~Form1()
        {
            var i = 0;
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            _engine.Global.Delete("log", false);
            _engine.Global.Delete("mainForm", false);
            _engine.Global.Delete("msgBoxOk", false);
            _engine.Global.Delete("msgBoxYesNo", false);
            _engine = null;
        }





        private void DebugTests()
        {
            _engine.SetValue("convertToTypedArray", new Func<object, object, object>((type, jsarray) =>
            {
                if (typeof(IList).IsAssignableFrom(jsarray.GetType()))
                {
                    var tempArray = System.Array.CreateInstance(typeof(object), (jsarray as IList).Count);
                    (jsarray as IList).CopyTo(tempArray, 0);
                    return tempArray;
                }
                else
                {
                    var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(type as Type);
                    var result = castMethod.Invoke(null, new[] { jsarray });
                    var arrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(type as Type);
                    return arrayMethod.Invoke(null, new[] { result });
                }
            }
            ));
            _engine.SetValue("convertToTypedArray2", new System.Func<Type, object, object>((type, jsarray) =>
            {
                if (typeof(IList).IsAssignableFrom(jsarray.GetType()))
                {
                    var tempArray = System.Array.CreateInstance(typeof(object), (jsarray as IList).Count);
                    (jsarray as IList).CopyTo(tempArray, 0);
                    return tempArray;
                    //return (tempArray as IEnumerable<object>).Where(x => x.GetType() == type).ToArray(); // TODO TEST!
                }
                else
                {
                    var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(type as Type);
                    var result = castMethod.Invoke(null, new[] { jsarray });
                    var arrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(type as Type);
                    return arrayMethod.Invoke(null, new[] { result });
                }
            }));

            var listControl = new System.Windows.Forms.ListView();
            listControl.Items.Add(new ListViewItem("item1") { Tag = "item1" });
            listControl.Items.Add(new ListViewItem("item2") { Tag = "item2" });

            var items =

            _engine.SetValue("listControl", listControl);
            RunJavascript(@"
//                var items = new Array();
//for (i = 0; i < listControl.Items; i++) {
//    items[i] = listControl.Items[i];
//}

		        //items.some(function(item) {
var items = convertToTypedArray2(System.Windows.Forms.ListViewItem, listControl.Items);
items.some(function(item) {
            msgBoxOk('item',item.Tag); 
		            var licensePlate = item.Tag;
		            if (licensePlate == 'item2')
		            {
			            foundItem = true;
			            return true;
		            }
	            });
msgBoxOk('','done');
            ");

            var returnValue = _engine.GetCompletionValue();
        }

        private void AddRetryFunction()
        {
            RunJavascript(@"
// Following the convention that:
// - For a giving function, the last argument is a callback 
//   And for that callback, the first argument is the error.
// This function allows us to get our original function to retry itself N times whenever there is failure 
// (when the first argument of the callback is true) before giving up.
// It overrides the original callback  function in order to test for the value of the first argument until it 
// is false (no error) or the retry cap is reached.
var retry = function(times, func) {
	var retries = 0;
	if(typeof times == ""function"") {
		func = times;
		times = 5; //Default
	}
	return function() {
		var args = arguments;
		function retry(original_fn) {
			 original_fn.apply(null,args);
		}

		var real_cb = args[String(args.length - 1)];
		if(typeof real_cb != ""function"") {
		  throw new Error(""Last argument expected to be a callback"");
		}
		args[String(args.length - 1)] = function() {
			var err = arguments['0'];
			if(err) {
				retries++;
				if(retries == times) {
				   real_cb.apply(null, arguments);
				} else {
					retry(func);
					return;
				}
			} else {
				real_cb.apply(null, arguments);
			}
		};
		func.apply(null, args)
	}
}");
        }
    }
}