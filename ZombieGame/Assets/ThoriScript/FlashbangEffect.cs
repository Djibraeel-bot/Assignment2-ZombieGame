using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class FlashbangEffect : MonoBehaviour
{
   public Volume volume;
   public CanvasGroup alphaController;

   private bool isFlashed = false;

   void Update()
   {
      if (isFlashed)
      {
         Time.timeScale = 0.05f;

         alphaController.alpha -= Time.unscaledDeltaTime * 2f;
         volume.weight -= Time.unscaledDeltaTime * 2f;

         if (alphaController.alpha <= 0)
         {
            alphaController.alpha = 0;
            volume.weight = 0;
            Time.timeScale = 1f;
            isFlashed = false;
         }
      }
   }

   public void FlashBanged()
   {
      volume.weight = 1f;
      alphaController.alpha = 1f;
      isFlashed = true;
   }
}
