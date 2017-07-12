using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class Settings
  {

    public void AddValidatorsTo(ValidatorSet validatorSet)
    {
      if (MaxWidth != null || MaxHeight != null)
      {
        validatorSet.AddValidator(new Sdx.Validation.Image.MaxSize(MaxWidth, MaxHeight));
      }

      if (MinWidth != null || MinHeight != null)
      {
        validatorSet.AddValidator(new Sdx.Validation.Image.MinSize(MinWidth, MinHeight));
      }

      if (Width != null || Height != null)
      {
        validatorSet.AddValidator(new Sdx.Validation.Image.Size(Width, Height));
      }

      if(Types != null)
      {
        validatorSet.AddValidator(new Sdx.Validation.Image.Type(Types));
      }

      if(MaxCapacity != null)
      {
        validatorSet.AddValidator(new Sdx.Validation.Image.MaxCapacity((int)MaxCapacity));
      }
    }

    public int? MaxWidth { get; set; }

    public int? MaxHeight { get; set; }

    public int? MinWidth { get; set; }

    public int? MinHeight { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public IEnumerable<Sdx.Image.Format> Types { get; set; }

    public int? MaxCapacity { get; set; }
  }
}
