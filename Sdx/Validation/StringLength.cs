﻿using NGettext;
using NGettext.Loaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sdx.Validation
{
  public class StringLength : Validator
  {
    public const string ErrorTooShort = "ErrorTooShort";
    public const string ErrorTooLong = "ErrorTooLong";

    protected override string GetDefaultMessage(string errorType)
    {
      var currentMessage = Sdx.I18n.GetPluralString("現在{0}文字", "現在{0}文字", AcutualLength, AcutualLength);
      switch (errorType)
      {
        case ErrorTooShort:
          return Sdx.I18n.GetString("{0}文字以上入力してください（{1}）。", Min, currentMessage);
        case ErrorTooLong:
          return Sdx.I18n.GetPluralString("{0}文字までしか入力できません（{1}）。", "{0}文字までしか入力できません（{1}）。", (long)Max, Max, currentMessage);
        default:
          return null;
      }
    }

    private long? min;
    private long? max;

    public long? Min
    {
      get
      {
        return this.min;
      }

      set
      {
        this.min = value;
        this.SetPlaceholder("min", value.ToString());
      }
    }

    public long? Max
    {
      get
      {
        return this.max;
      }

      set
      {
        this.max = value;
        this.SetPlaceholder("max", this.max.ToString());
      }
    }

    public StringLength(long? min = null, long? max = null, string message = null)
      : base(message)
    {
      if(min == null && max == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }
      this.Min = min;
      this.Max = max;
    }


    protected override bool IsValidString(string value)
    {
      int length = value.Length;
      this.SetPlaceholder("actual_length", length.ToString());
      this.AcutualLength = length;

      if (this.Min != null && length < this.Min)
      {
        this.AddError(ErrorTooShort);
        return false;
      }

      if (this.Max != null && this.Max < length)
      {
        this.AddError(ErrorTooLong);
        return false;
      }

      return true;
    }

    public long AcutualLength { get; private set; }
  }
}
