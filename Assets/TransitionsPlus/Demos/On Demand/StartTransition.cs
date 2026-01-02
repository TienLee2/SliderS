using TransitionsPlus;
using UnityEngine;

namespace TransitionsPlusDemos
{

    public class StartTransition : MonoBehaviour
    {

        public Texture2D picture;

        public TransitionProfile starTransitionProfile;

        void OnEnable()
        {
            InputProxy.SetupEventSystem();
        }

        public void StartFadeTransition()
        {
            TransitionAnimator.Start(
                TransitionType.Fade,     // transition type
                duration: 2f,            // transition duration in seconds
                noiseIntensity: 0.2f     // intensity of noise
                );
        }


        public void StartStarCartoonTransition()
        {
            TransitionAnimator.Start(TransitionType.Shape, shapeTexture: Resources.Load<Texture2D>("Textures/StartSDF"), splits: 1, keepAspectRatio: true, rotationMultiplier: 2f, duration: 2);
        }

        public void StartStarCartoon2Transition()
        {
            TransitionAnimator.Start(starTransitionProfile);
        }

        public void StartWipeTransition()
        {
            TransitionAnimator.Start(TransitionType.Wipe, duration: 2, noiseIntensity: 0.1f, rotation: -15f);
        }

        public void StartCrossWipeTransition()
        {
            TransitionAnimator.Start(TransitionType.CrossWipe, rotationMultiplier: 5f, duration: 2);
        }

        public void StartDoubleWipeTransition()
        {
            TransitionAnimator.Start(TransitionType.DoubleWipe, duration: 2);
        }

        public void StartMosaicTransition()
        {
            TransitionAnimator.Start(TransitionType.Mosaic, duration: 2, cellsDivisions: 6, spread: 8, texture: picture);
        }

        public void StartDissolveTransition()
        {
            TransitionAnimator.Start(TransitionType.Dissolve, duration: 2, cellsDivisions: 128);
        }

        public void StartBurnTransition()
        {
            TransitionAnimator.Start(TransitionType.Burn, duration: 2, color: new Color(0.5f, 0, 0));
        }

        public void StartBurnSquareTransition()
        {
            TransitionAnimator.Start(TransitionType.BurnSquare, duration: 2, contrast: 500f);
        }

        public void StartTilesProgressive()
        {
            TransitionAnimator.Start(TransitionType.TilesProgressive, duration: 2, cellsDivisions: 5);
        }

        public void StartCircularWipeTransition()
        {
            TransitionAnimator.Start(TransitionType.CircularWipe, duration: 2, contrast: 10f, noiseIntensity: 0.2f, toonGradientIntensity: 16);
        }

        public void StartSeaWavesTransition()
        {
            TransitionAnimator.Start(TransitionType.SeaWaves, duration: 2, rotationMultiplier: 0.5f, splits: 4);
        }

        public void StartSplashTransition()
        {
            TransitionAnimator.Start(TransitionType.Splash, duration: 2);
        }

        public void StartTilesTransition()
        {
            TransitionAnimator.Start(TransitionType.Tiles, duration: 2, cellsDivisions: 8, rotationMultiplier: 2f, contrast: 50f, noiseIntensity: 0);
        }

        public void StartCirclesTransition()
        {
            TransitionAnimator.Start(TransitionType.Circles, duration: 2, cellsDivisions: 8, rotationMultiplier: 2f, contrast: 50f, noiseIntensity: 0);
        }

        public void StartSmearTransition()
        {
            TransitionAnimator.Start(TransitionType.Smear, duration: 2);
        }

        public void StartPixelateTransition()
        {
            TransitionAnimator.Start(TransitionType.Pixelate, duration: 2);
        }

        public void StartSlideTransition()
        {
            TransitionAnimator.Start(TransitionType.Slide, duration: 2);
        }

        public void StartDoubleSlideTransition()
        {
            TransitionAnimator.Start(TransitionType.DoubleSlide, rotation: 90, duration: 2);
        }

        public void StartCubeRotationTransition()
        {
            TransitionAnimator.Start(TransitionType.Cube, texture: picture, duration: 2);
        }

        public void StartSpiralTransition()
        {
            TransitionAnimator.Start(TransitionType.Spiral, cellsDivisions: 9, spread: 16, duration: 2);
        }

        public void StartWarpTransition()
        {
            TransitionAnimator.Start(TransitionType.Warp, duration: 2);
        }

        public void StartRippleTransition()
        {
            TransitionAnimator.Start(TransitionType.Ripple, duration: 2);
        }

    }
}

