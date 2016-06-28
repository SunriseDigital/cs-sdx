using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web.Image
{
  public class S3 : Base
  {
    public S3()
    {
    }

    public string AwsAccessKey
    {
      get
      {
        return this.AwsAccessKey;
      }

      set
      {
        this.AwsAccessKey = value;
      }
    }

    public string AwsSecretKey
    {
      get
      {
        return this.AwsSecretKey;
      }

      set
      {
        this.AwsSecretKey = value;
      }
    }

    public string AwsRegion
    {
      get
      {
        return this.AwsRegion;
      }

      set
      {
        this.AwsRegion = value;
      }
    }

    public string BucketName
    {
      get
      {
        return this.BucketName;
      }

      set
      {
        this.BucketName = value;
      }
    }
  }
}
