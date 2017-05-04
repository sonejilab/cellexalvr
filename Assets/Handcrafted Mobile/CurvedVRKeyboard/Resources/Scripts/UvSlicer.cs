//#define DEBUG_UV_SLICER

using System.Collections.Generic;
using UnityEngine;

namespace CurvedVRKeyboard {
    public struct Border {
        public float left;
        public float right;
        public float top;
        public float bottom;

        public Border(float left, float right, float top, float bottom) {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public void reset (float left, float right, float top, float bottom) {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
}

    public class UvSlicer {
        public Vector3 verticalVector { get; private set; }

        public float referencedPixels = 10f;

        public Border objectBorderInUnits = new Border(-2f, 2f, 0.5f, -0.5f);
        public Border uvBorderInPercent = new Border(-2f, 2f, 0.5f, -0.5f);

        private Vector2 size;


        public void ChangeSprite(Sprite spaceSprite) {
            CalculateBordersAndSize(spaceSprite);
        }

        private void CalculateBordersAndSize(Sprite spaceSprite) {
            if(spaceSprite != null) {
                
                size = new Vector2(spaceSprite.bounds.size.x, spaceSprite.bounds.size.y);
                size = size * 10 * referencedPixels ;


                uvBorderInPercent.left = ( spaceSprite.border.x / spaceSprite.bounds.size.x ) / 100f;
                objectBorderInUnits.left = ( spaceSprite.border.x / size.x ) - 2f;

                uvBorderInPercent.right = 1f - ( ( spaceSprite.border.z ) / spaceSprite.bounds.size.x ) / 100f;
                objectBorderInUnits.right = ( 1f - spaceSprite.border.z / size.x ) + 1f;

                uvBorderInPercent.bottom = ( spaceSprite.border.y / spaceSprite.bounds.size.y ) / 100f;
                objectBorderInUnits.bottom = ( spaceSprite.border.y ) / size.y - 0.5f;

                uvBorderInPercent.top = 1f - ( ( spaceSprite.border.w ) / spaceSprite.bounds.size.y ) / 100f;
                objectBorderInUnits.top = ( 1f - spaceSprite.border.w / size.y ) - 0.5f;

                if(objectBorderInUnits.top < objectBorderInUnits.bottom) {
                    float borderTopPerc = 1f - uvBorderInPercent.top;
                    float borderSum = borderTopPerc + uvBorderInPercent.bottom;
                    float borderTopRatio = borderTopPerc / borderSum;
                    objectBorderInUnits.top = 1f -( 1f * borderTopRatio) - 0.51f;
                    objectBorderInUnits.bottom = objectBorderInUnits.top + 0.02f;
                }
                if(objectBorderInUnits.left > objectBorderInUnits.right) {
                    float borderLeftPerc = 1f - uvBorderInPercent.left;
                    float borderSum = borderLeftPerc + uvBorderInPercent.right;
                    float borderLeftRatio = borderLeftPerc / borderSum;
                    objectBorderInUnits.left = (1f - ( 1f * borderLeftRatio )) * 4f - 2.01f;
                    objectBorderInUnits.right = objectBorderInUnits.left + 0.02f;
                }
                if(objectBorderInUnits.left % 1.00f == 0) {
                    objectBorderInUnits.left += 0.01f;
                }
                if(objectBorderInUnits.right % 1.00f == 0) {
                    objectBorderInUnits.right -= 0.01f;
                }
            } else {
                objectBorderInUnits.reset(-2f, 2f, 0.5f, -0.5f);
                uvBorderInPercent.reset(0, 1f, 1f, 0);
            }

#if DEBUG_UV_SLICER
            Debug.Log("");
            Debug.Log("");
            Debug.Log(string.Format("Top {0} : bottom {1} : left {2} , right{3}", top, bot, left, right));
            Debug.Log(string.Format("top% {0} : bottom% {1} : left% {2} , right% {3}", percentageTop, percentageBottom, percentageLeft, percentageRight));
#endif
        }

        public void CheckVerticalBorders(float current, float spacing,SpaceMeshCreator spaceMeshCreator) {
            if(current <= objectBorderInUnits.left && objectBorderInUnits.left <= current + spacing) { // left border shall be added??
                spaceMeshCreator.AddWholeColumn(new Vector3(objectBorderInUnits.left, 0, 0));
            }
            if(objectBorderInUnits.right > current && objectBorderInUnits.right <= current + spacing) {
                spaceMeshCreator.AddWholeColumn(new Vector3(objectBorderInUnits.right, 0, 0));
            }

        }

        public Vector2[] BuildUV(List<Vector3> verticiesArray, Vector2 boundary) {
            float leftSize = Mathf.Abs(objectBorderInUnits.left - (-boundary.x)) + Mathf.Epsilon;
            float rightSize = Mathf.Abs(boundary.x - objectBorderInUnits.right) + Mathf.Epsilon;
            float midSize = boundary.x * 2 - leftSize - rightSize;

            Vector2[] calculatedUV = new Vector2[verticiesArray.Count];
            calculatedUV[0] = new Vector2(0f, 1f);
            calculatedUV[1] = new Vector2(0f, uvBorderInPercent.top);
            calculatedUV[2] = new Vector2(0f, uvBorderInPercent.bottom);
            calculatedUV[3] = new Vector2(0f, 0f);

            for(int row = 4; row < verticiesArray.Count; row += 4) {
                float percentageReal = 0f;
                if(verticiesArray[row].x <= objectBorderInUnits.left) {
                    float positionInLowScale = verticiesArray[row].x + boundary.x;
                    float percentageInLowScale = positionInLowScale / leftSize;
                    percentageReal = Mathf.Lerp(0f, uvBorderInPercent.left, percentageInLowScale);
                }
                else if(verticiesArray[row].x >= objectBorderInUnits.right) {
                    float positionInLowScale = verticiesArray[row].x - objectBorderInUnits.right;
                    float percentageInLowScale = positionInLowScale / rightSize;
                    percentageReal = Mathf.Lerp(uvBorderInPercent.right, 1f, percentageInLowScale);
                }
                else {
                    float positionInLowScale = verticiesArray[row].x - objectBorderInUnits.left;
                    float percentageInLowScale = positionInLowScale / midSize;
                    percentageReal = Mathf.Lerp(uvBorderInPercent.left, uvBorderInPercent.right, percentageInLowScale);
                }

                calculatedUV[row] = new Vector2(percentageReal, 1f);
                calculatedUV[row + 1] = new Vector2(percentageReal, uvBorderInPercent.top);
                calculatedUV[row + 2] = new Vector2(percentageReal, uvBorderInPercent.bottom);
                calculatedUV[row + 3] = new Vector2(percentageReal, 0f);
            }

            return calculatedUV;
        }

    }
}
