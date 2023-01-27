using yak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

public class AtributeDisplay : MonoBehaviour
{
    public GameObject _controlTarget; // public field for unity editor...
    public GameObject controlTarget   // for code...
    {
        get { return _controlTarget; }
        set { _controlTarget = value; initControls(); }
    }

    private object _objTarget;
    public object objTarget// for code...
    {
        get { return _objTarget; }
        set { _objTarget = value; initControls(); }
    }

    private object usedObjTarget;

    [SerializeField]
    public CtrlLevel level;

    [Space]
    [Space]
    public GameObject controlsGrp;
    [Space]
    public GameObject templatesGrp;
    public GameObject prefabText;
    public GameObject prefabToggle;
    public GameObject prefabSlider;
    public GameObject prefabInputField;

    private Dictionary<Ctrl.CtrlConnection, Text> controlledFields = new Dictionary<Ctrl.CtrlConnection, Text>();
    private Dictionary<Ctrl.CtrlConnection, Slider> sliders = new Dictionary<Ctrl.CtrlConnection, Slider>();
    private Dictionary<Ctrl.CtrlConnection, Image> colors = new Dictionary<Ctrl.CtrlConnection, Image>();
    private Dictionary<Ctrl.CtrlConnection, InputField> inputfields = new Dictionary<Ctrl.CtrlConnection, InputField>();
    private Dictionary<Ctrl.CtrlConnection, Toggle> toggles = new Dictionary<Ctrl.CtrlConnection, Toggle>();

    private void Awake()
    {
        templatesGrp.SetActive(false);
        controlsGrp.SetActive(true);

        initControls();
    }

    private void Update()
    {
        foreach (var item in controlledFields)
        {
            item.Value.gameObject.SetActive(level >= item.Key.attr.level);

            string name = item.Key.attr.label;
            if (string.IsNullOrEmpty(name))
                name = item.Key.propertyName;

            item.Value.text = name + " :" + item.Key.getValue<string>();
        }

        foreach (var item in colors)
        {
            item.Value.gameObject.SetActive(level >= item.Key.attr.level);

            item.Value.color = item.Key.getValue<Color>();
        }

        foreach (var item in sliders)
        {
            item.Value.gameObject.SetActive(level >= item.Key.attr.level);

            var attr_ = item.Key.attr as CtrlSlider;
            item.Value.value = item.Key.getValue<float>();
        }

        foreach (var item in inputfields)
        {
            item.Value.gameObject.SetActive(level >= item.Key.attr.level);

            //don't disterb editing
            if (!item.Value.isFocused)
                item.Value.text = item.Key.getValue<string>();
        }

        foreach (var item in toggles)
        {
            item.Value.gameObject.SetActive(level >= item.Key.attr.level);
            item.Value.isOn = item.Key.getValue<bool>();
        }
    }

    // -------------------------------------------------------------------
    // -------------------------------------------------------------------
    private void initControls()
    {
        //remove old children
        for (int i = controlsGrp.transform.childCount - 1; i >= 0; i--)
            Destroy(controlsGrp.transform.GetChild(i).gameObject);

        controlledFields.Clear();
        sliders.Clear();
        colors.Clear();
        inputfields.Clear();

        List<object> objs = new List<object>();

        //using component
        if (controlTarget != null)
        {
            objs.AddRange(controlTarget.GetComponents<Component>());
        }

        if (objTarget != null)
        {
            objs.Add(objTarget);
        }

        //create :
        var cons = Ctrl.getForObjects(objs);
        foreach (var con in cons)
        {
            //default
            {
                //var attr_ = con.attr as CtrlSlider;
                var obj = Instantiate(prefabText);
                obj.name = con.propertyName;
                obj.transform.SetParent(controlsGrp.transform,false);
                var ui = obj.GetComponent<Text>();
                controlledFields.Add(con, ui);
            }

            if (con.fieldType == typeof(Color))
            {
                var obj = new GameObject();
                obj.name = con.propertyName;
                obj.transform.SetParent(controlsGrp.transform, false);
                var ui = obj.AddComponent<Image>();
                colors.Add(con, ui);
            }

            if (con.attr.GetType() == typeof(CtrlSlider))
            {
                var attr_ = con.attr as CtrlSlider;
                var obj = Instantiate(prefabSlider);
                obj.name = con.propertyName;
                obj.transform.SetParent(controlsGrp.transform, false);
                var ui = obj.GetComponent<Slider>();
                ui.minValue = attr_.min;
                ui.maxValue = attr_.max;
                sliders.Add(con, ui);

                if (con.fieldType == typeof(int))
                    ui.wholeNumbers = true;
                if (con.fieldType == typeof(long))
                    ui.wholeNumbers = true;
                if (con.fieldType == typeof(float))
                    ui.wholeNumbers = false;
                if (con.fieldType == typeof(double))
                    ui.wholeNumbers = false;

                ui.onValueChanged.AddListener((x) =>
                {
                    con.setValue(x);
                });
            }

            if (con.attr.GetType() == typeof(CtrlInputText))
            {
                var attr_ = con.attr as CtrlInputText;
                var obj = Instantiate(prefabInputField);
                obj.name = con.propertyName;
                obj.transform.SetParent(controlsGrp.transform, false);
                var ui = obj.GetComponent<InputField>();
                inputfields.Add(con, ui);

                if (con.fieldType == typeof(int))
                    ui.contentType = InputField.ContentType.IntegerNumber;
                if (con.fieldType == typeof(long))
                    ui.contentType = InputField.ContentType.IntegerNumber;
                else if (con.fieldType == typeof(float))
                    ui.contentType = InputField.ContentType.DecimalNumber;
                else if (con.fieldType == typeof(double))
                    ui.contentType = InputField.ContentType.DecimalNumber;
                else
                    ui.contentType = InputField.ContentType.Standard;

                ui.onEndEdit.AddListener((x) =>
                {
                    con.setValue(x);
                });
            }

            if (con.attr.GetType() == typeof(CtrlToggle))
            {
                var attr_ = con.attr as CtrlToggle;
                var obj = Instantiate(prefabToggle);
                obj.name = con.propertyName;
                obj.transform.SetParent(controlsGrp.transform, false);
                var ui = obj.GetComponent<Toggle>();

                var text = ui.GetComponentInChildren<Text>();
                if (text != null)
                {
                    string name = attr_.label;
                    if (string.IsNullOrEmpty(name))
                        name = con.propertyName;

                    text.text = name;
                }

                toggles.Add(con, ui);

                ui.onValueChanged.AddListener((x) =>
                {
                    con.setValue(x);
                });
            }
        }
    }
}