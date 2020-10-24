// This file was @generated with LibOVRPlatform/codegen/main. Do not modify it!

namespace Oculus.Platform.Models
{
  using System;
  using System.Collections;
  using Oculus.Platform.Models;
  using System.Collections.Generic;
  using UnityEngine;

  public class Leaderboard
  {
    public readonly string ApiName;


    public Leaderboard(IntPtr o)
    {
      ApiName = CAPI.ovr_Leaderboard_GetApiName(o);
    }
  }

}
