using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using System.Runtime.CompilerServices;
using DynamicSettings.Editors;
using DynamicSettings.Layout;
using System.Configuration;

namespace DynamicSettings
{
    public partial class DynamicSettingsControl : UserControl
    {
        private Dictionary<string, Panel> panels = new Dictionary<string, Panel>();
        private ICollection<Control> editors = new List<Control>();
        private SettingsLayout layout;
        private DataTable evaluator;

        /// <summary>
        /// Gets or sets settings object which properties to be edited with the control UI editors.
        /// </summary>
        [Browsable(false)]
        public object Settings { get; set; }

        /// <summary>
        /// Gets dictionary of settings editors builders.
        /// Key is an editor type, value is a function which provides control.
        /// </summary>
        [Browsable(false)]
        public IDictionary<string, Func<SettingNode, Control>> EditorBuilders { get; } = new Dictionary<string, Func<SettingNode, Control>>();

        /// <summary>
        /// Gets dictionary of default editors.
        /// Key is a setting type, value is a default editor that should be created for this setting type.
        /// </summary>
        [Browsable(false)]
        public IDictionary<Type, string> DefaultEditors { get; } = new Dictionary<Type, string>();

        [Category("UI")]
        [Description("Specifies application configuration section to load settings layout from.")]
        public string LayoutConfigSection { get; set; }

        public void LoadLayoutFromFile(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open))
            {
                LoadLayoutFromStream(stream);
            }
        }

        public void LoadLayoutFromStream(Stream stream)
        {
            layout = SettingsLayout.Load(stream);
        }

        public void LoadLayoutFromConfigSection(string configSectionName)
        {
            layout = ConfigurationManager.GetSection(configSectionName) as SettingsLayout;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (!DesignMode)
            {
                if (layout == null && !string.IsNullOrWhiteSpace(LayoutConfigSection))
                {
                    LoadLayoutFromConfigSection(LayoutConfigSection);
                }

                if (layout == null)
                {
                    throw new DynamicSettingsException($"Settings layout was not loaded. Please load it explicitly by calling one of `LoadLayoutFrom...()` methods or by specifying `LayoutConfigSection` property to load from application configuration file.");
                }

                CreateControls();
                UpdateControls();
            }
        }

        public DynamicSettingsControl()
        {
            InitializeComponent();
            evaluator = new DataTable();

            DefaultEditors.Add(typeof(bool), "checkbox");
            DefaultEditors.Add(typeof(string), "textbox");
            DefaultEditors.Add(typeof(Color), "colorpicker");
            DefaultEditors.Add(typeof(Font), "fontpicker");

            #region Default Control Builders

            // Checkbox Builder
            EditorBuilders.Add("checkbox", setting =>
            {
                var checkBox = new CheckBox()
                {
                    Text = setting.Title,
                    Checked = GetSettingValue<bool>(setting.Name),
                    AutoSize = true,
                    UseVisualStyleBackColor = true
                };
                checkBox.CheckedChanged += (s, e) => SetSettingValue(setting.Name, checkBox.Checked);
                return checkBox;
            });

            // Radio Group Builder
            EditorBuilders.Add("radio", setting =>
            {
                var group = new GroupBox()
                {
                    AutoSize = true,
                    Text = setting.Title,
                    Dock = DockStyle.Top
                };

                FlowLayoutPanel panel = new FlowLayoutPanel()
                {
                    AutoSize = true,
                    FlowDirection = FlowDirection.TopDown,
                    Dock = DockStyle.Fill
                };

                object settingValue = GetSettingValue<object>(setting.Name);
                Array values = Enum.GetValues(settingValue.GetType());
                foreach (var value in values)
                {
                    RadioButton radio = new RadioButton()
                    {
                        Name = $"{setting.Name}.{value}",
                        AutoSize = true,
                        Checked = value.Equals(settingValue),
                        Text = value.GetType()
                            .GetMember(value.ToString())
                            .FirstOrDefault()
                            ?.GetCustomAttribute<DescriptionAttribute>()
                            ?.Description
                    };
                    radio.CheckedChanged += (s, e) => { if (radio.Checked) { SetSettingValue(setting.Name, value); } };

                    panel.Controls.Add(radio);
                }

                group.Controls.Add(panel);

                return group;
            });

            // Textbox Builder
            EditorBuilders.Add("textbox", setting => 
            {
                TextPicker picker = new TextPicker()
                {
                    Title = setting.Title,
                    SelectedValue = GetSettingValue<string>(setting.Name),
                    Dock = DockStyle.Fill
                };
                picker.SelectedValueChanged += (s, e) => SetSettingValue(setting.Name, picker.SelectedValue);

                return picker;
            });

            // Color Picker Builder
            EditorBuilders.Add("colorpicker", setting =>
            {
                ColorPicker picker = new ColorPicker()
                {
                    Text = setting.Title,
                    SelectedValue = GetSettingValue<Color>(setting.Name),
                    AutoSize = true
                };
                picker.SelectedValueChanged += (s, e) => SetSettingValue(setting.Name, picker.SelectedValue);

                return picker;
            });

            // Font Picker Builder
            EditorBuilders.Add("fontpicker", setting =>
            {
                Font font = GetSettingValue<Font>(setting.Name);
                FontPicker picker = new FontPicker()
                {
                    Title = setting.Title,
                    SelectedValue = font,
                    Dock = DockStyle.Fill
                };

                picker.SelectedValueChanged += (s, e) => SetSettingValue(setting.Name, picker.SelectedValue);

                return picker;
            });

            // Directory Picker Builder
            EditorBuilders.Add("directorypicker", setting =>
            {
                string directory = GetSettingValue<string>(setting.Name);
                DirectoryPicker picker = new DirectoryPicker()
                {
                    Title = setting.Title,
                    SelectedValue = directory,
                    Dock = DockStyle.Fill
                };

                picker.SelectedValueChanged += (s, e) => SetSettingValue(setting.Name, picker.SelectedValue);

                return picker;
            });

            #endregion Default Control Builders
        }

        private void CreateControls()
        {
            foreach (var section in layout.Sections)
            {
                if (string.IsNullOrWhiteSpace(section.Name))
                {
                    throw new DynamicSettingsException("Section should have non-empty name.");
                }

                foreach (var setting in section.Settings)
                {
                    if (string.IsNullOrWhiteSpace(setting.Name))
                    {
                        throw new DynamicSettingsException($"Setting should have non-empty name.");
                    }
                }
            }

            foreach (var section in layout.Sections)
            {
                listSections.Items.Add(section.Title ?? section.Name);
                var panel = new TableLayoutPanel()
                {
                    Visible = false,
                    AutoScroll = true,
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                };
               
                foreach (var setting in section.Settings)
                {
                    panel.Controls.Add(BuildSettingControl(setting));
                }

                splitContainer.Panel2.Controls.Add(panel);

                panels[section.Name] = panel;
            }

            listSections.SelectedIndex = 0;
        }

        private void UpdateControls()
        {
            var values = new Dictionary<string, object>();
            foreach (var n in layout.Sections.SelectMany(n => n.Settings))
            {
                values.Add(n.Name, GetSettingValue<object>(n.Name));
            }

            var nodes = layout.Sections.SelectMany(n => n.Settings);
            
            foreach (var n in nodes)
            {
                var control = editors.FirstOrDefault(c => c.Name == n.Name);
                if (control != null)
                {
                    if (!string.IsNullOrWhiteSpace(n.EnabledIf))
                    {
                        var result = EvaluateExpression(n.EnabledIf, values);
                        if (result is bool)
                        {
                            control.Enabled = (bool)result;
                        }
                        else
                        {
                            throw new DynamicSettingsException($"Expression `enabledIf` for setting `{n.Name}` should have boolean type.");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(n.VisibleIf))
                    {
                        var result = EvaluateExpression(n.VisibleIf, values);
                        if (result is bool)
                        {
                            control.Visible = (bool)result;
                        }
                        else
                        {
                            throw new DynamicSettingsException($"Expression `visibleIf` for setting `{n.Name}` should have boolean type.");
                        }
                    }
                }
            }
        }

        private object EvaluateExpression(string expression, Dictionary<string, object> values)
        {
            foreach (var item in values)
            {
                expression = expression.Replace($"{{{item.Key}}}", item.Value.ToString());
            }

            return evaluator.Compute(expression, null);
        }

        private Control BuildSettingControl(SettingNode setting)
        {
            Control control = null;

            string editorName = setting.Editor;

            if (string.IsNullOrEmpty(editorName))
            {
                var settingType = GetSettingValue<object>(setting.Name).GetType();
                editorName = DefaultEditors.ContainsKey(settingType) ? DefaultEditors[settingType] : null;
            }

            if (editorName == null)
            {
                throw new DynamicSettingsException($"Setting `{setting.Name}` has undefined editor. Make sure you have specified `editor` attribute in settings layout.");
            }

            if (EditorBuilders.ContainsKey(editorName))
            {
                control = EditorBuilders[editorName].Invoke(setting);
            }
            else
            {
                throw new DynamicSettingsException($"Setting `{setting.Name}` has unknown editor `{editorName}`. Make sure you have added the editor builder function with key `{editorName}` to `EditorBuilders` dictionary.");
            }

            control.Name = setting.Name;

            editors.Add(control);
            return control;
        }

        private void listSections_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listSections.SelectedIndex == -1)
            {
                listSections.SelectedIndex = 0;
            }
            else
            {
                var section = layout.Sections.ElementAt(listSections.SelectedIndex);
                var panel = panels[section.Name];
                foreach (var p in panels.Values)
                {
                    p.Visible = p == panel;
                }
            }
        }

        public T GetSettingValue<T>(string settingName)
        {
            if (Settings != null)
            {
                if (Settings is DynamicObject)
                {
                    return (T)ReflectionUtils.GetDynamicProperty(Settings, settingName);
                }
                else
                {
                    return (T)ReflectionUtils.GetObjectProperty(Settings, settingName);
                }
            }
            else
            {
                throw new DynamicSettingsException("`Settings` property of the contol is not set.");
            }
        }

        public void SetSettingValue(string settingName, object value)
        {
            if (Settings != null)
            {
                if (Settings is DynamicObject)
                {
                    ReflectionUtils.SetDynamicProperty(Settings, settingName, value);
                }
                else
                {
                    ReflectionUtils.SetObjectProperty(Settings, settingName, value);
                }
                UpdateControls();
            }
            else
            {
                throw new DynamicSettingsException("`Settings` property of the contol is not set.");
            }
        }
    }
}
