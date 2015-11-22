
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MinskTrans.Utilites.Base.Net;
    using MinskTrans.Universal.Annotations;
    using Windows.Storage;

//using MinskTrans.DesctopClient.Annotations;
//using MinskTrans.DesctopClient.Annotations;
using MyLibrary;

namespace MinskTrans.DesctopClient.Modelview
{
    public class UniversalApplicationSettingsHelper : IApplicationSettingsHelper
    {
        readonly Dictionary<string, DateTime> dateTimeDictionary = new Dictionary<string, DateTime>();

        public void SimpleSet<T>(Error value, string key = null)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                ApplicationData.Current.LocalSettings.Values.Add(key, (int)value);
            else
                ApplicationData.Current.LocalSettings.Values[key] = (int)value;
        }

        public DateTime SimbleGet(DateTime value = default(DateTime), [CallerMemberName] string key = null)
        {
            if (dateTimeDictionary.ContainsKey(key))
            {
                if (dateTimeDictionary[key] != default(DateTime))
                    return dateTimeDictionary[key];
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    var backField =
                        DateTime.Parse(ApplicationData.Current.LocalSettings.Values[key].ToString());
                    dateTimeDictionary.Add(key, backField);
                    return backField;
                }

            }
            return value;
        }

        public void SimpleSet<T>(T value, [CallerMemberName] string key = null)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            else
                ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        public T SimpleGet<T>(T defValue = default(T), [CallerMemberName] string key = null)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                ApplicationData.Current.LocalSettings.Values.Add(key, defValue);
            return (T)ApplicationData.Current.LocalSettings.Values[key];
        }

        public Error SimpleGet(Error defValue = default(Error), [CallerMemberName] string key = null)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                ApplicationData.Current.LocalSettings.Values.Add(key, (int)defValue);
            return (Error)ApplicationData.Current.LocalSettings.Values[key];
        }

        public void SimpleSet(DateTime value, [CallerMemberName] string key = null)
        {
            if (dateTimeDictionary.ContainsKey(key) && dateTimeDictionary[key] == value)
                return;
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value.ToString());
                dateTimeDictionary.Add(key, value);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value.ToString();
                dateTimeDictionary[key] = value;
            }
        }

        //public void SimpleSet(string value, [CallerMemberName]string key = null)
        // {
        //     if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
        //         ApplicationData.Current.LocalSettings.Values.Add(key, value);
        //     else
        //         ApplicationData.Current.LocalSettings.Values[key] = value;
        // }

        // public void SimpleSet(bool value, [CallerMemberName]string key = null)
        // {
        //     if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
        //         ApplicationData.Current.LocalSettings.Values.Add(key, value);
        //     else
        //         ApplicationData.Current.LocalSettings.Values[key] = value;
        // }

        // public void SimpleSet(int value, [CallerMemberName]string key = null)
        // {
        //     if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
        //         ApplicationData.Current.LocalSettings.Values.Add(key, value);
        //     else
        //         ApplicationData.Current.LocalSettings.Values[key] = value;
        // }

        // public string SimleGet(string defValue = null, [CallerMemberName]string key = null)
        // {

        //     return (string)SimleGet((object)defValue, key);
        // }

        // public object SimleGet(object defValue = null, [CallerMemberName]string key = null)
        // {
        //     if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
        //         ApplicationData.Current.LocalSettings.Values.Add(key, defValue);
        //     return ApplicationData.Current.LocalSettings.Values[key];
        // }

        // public bool SimleGet(bool defValue = true, [CallerMemberName]string key = null)
        // {

        //     return (bool)SimleGet((object)defValue, key);
        // }

        // public int SimleGet(int defValue = 0, [CallerMemberName]string key = null)
        // {
        //     return (int)SimleGet((object)defValue, key);
        // }

    }

    
}