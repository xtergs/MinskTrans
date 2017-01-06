using System;
using System.Runtime.CompilerServices;

namespace MinskTrans.Context.Utilites
{
    public interface IApplicationSettingsHelper
    {
        // DateTime DateTimeSettings { get; set; }
        void SimpleSet<T>(T value, [CallerMemberName] string key = null);
        T SimpleGet<T>(T defValue = default(T), [CallerMemberName] string key = null);
        Error SimpleGet(Error defValue = default(Error), [CallerMemberName] string key = null);
        void SimpleSet<T>(Error value, [CallerMemberName] string key = null);
        //void SimpleSet(string value, [CallerMemberName] string key = null);
        //void SimpleSet(bool value, [CallerMemberName] string key = null);
        //void SimpleSet(int value, [CallerMemberName] string key = null);
        //string SimleGet(string defValue = null, [CallerMemberName] string key = null);
        //object SimleGet(object defValue = null, [CallerMemberName] string key = null);
        //bool SimleGet(bool defValue = true, [CallerMemberName] string key = null);
        //int SimleGet(int defValue = 0, [CallerMemberName] string key = null);
        DateTime SimbleGet(DateTime value = default(DateTime), [CallerMemberName] string key = null);
        void SimpleSet(DateTime value, [CallerMemberName] string key = null);
        T SimpleEnumGet<T>(Enum defValue = default(Enum), [CallerMemberName] string key = null);
        void SimpleEnumSet<T>(Enum value, [CallerMemberName] string key = null);


    }
}