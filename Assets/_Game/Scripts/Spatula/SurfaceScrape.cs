using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
using ClocknestGames.Library.Utils;
using System.Linq;

namespace ClocknestGames.Game.Core
{
    public enum SurfaceScrapePartType
    {
        Road,
        Scraped
    }

    [System.Serializable]
    public enum SurfaceScrapeType
    {
        Default,
        LevelFinish
    }

    public class SurfaceScrapePartSetting
    {
        public SurfaceScrapePart ScrapePart;
        public Vector3 Scale;
        public Vector3 Offset;
    }

    public class SurfaceScrapeCreatePart
    {
        public SurfaceScrapePartType Type = SurfaceScrapePartType.Road;
        public SplineMesh Mesh;
        public MeshRenderer Renderer;
        public double ClipFrom = 0f;
        public double ClipTo = 1f;

        // INFO: Because of UV stretch problem, it is required to change channel's clip range instead of spline mesh.
        public virtual void SetClipRange(double clipFrom, double clipTo)
        {
            ClipFrom = clipFrom;
            ClipTo = clipTo;

            for (int index = 0; index < Mesh.GetChannelCount(); index++)
            {
                var channel = Mesh.GetChannel(index);
                channel.clipFrom = clipFrom;
                channel.clipTo = clipTo;
            }
        }

        public virtual void SetClipFrom(double clipFrom)
        {
            SetClipRange(clipFrom, ClipTo);
        }

        public virtual void SetClipTo(double clipTo)
        {
            SetClipRange(ClipFrom, clipTo);
        }
    }

    public class SurfaceScrape : Surface
    {
        public SurfaceScrapeType Type = SurfaceScrapeType.Default;
        public SplineComputer Spline;
        public Material SpiralMainMaterial;
        public Material SpiralSurfaceMaterial;
        [ColorUsage(true)] public Color ParticleMainColor;
        [ColorUsage(true, true)] public Color ParticleEmissionColor;
        public SplineMesh SurfaceScrapePartPrefab;
        public SplineMesh SurfaceScrapePartHiddenPrefab;
        public float SpiralWidth = 5f;

        public float SurfaceScale = .4f;
        public float SurfaceScrapeScale = .2f;

        public bool IsScraping { get; protected set; } = false;

        protected List<SurfaceScrapePartSetting> _surfaceParts;
        protected Dictionary<int, List<SurfaceScrapeCreatePart>> _surfaceCreatedParts;
        protected List<SurfaceScrapeCreatePart> _currentSurfaceCreatedParts => _surfaceCreatedParts[_currentSurfacePartIndex];
        protected int _currentSurfacePartIndex = 0;
        protected int _currentSurfaceCreatedPartIndex = 0;

        protected static SplineSample _tmpSplineSample = new SplineSample();

        protected virtual void Start()
        {
            _surfaceParts = new List<SurfaceScrapePartSetting>();
            _surfaceCreatedParts = new Dictionary<int, List<SurfaceScrapeCreatePart>>();

            var parts = GetComponentsInChildren<SurfaceScrapePart>().ToList();
            foreach (var part in parts)
            {
                var partMesh = part.Mesh.GetChannel(0).NextMesh();
                _surfaceParts.Add(new SurfaceScrapePartSetting { ScrapePart = part, Offset = partMesh.offset, Scale = partMesh.scale });
            }

            for (int index = 0; index < _surfaceParts.Count; index ++)
            {
                var surfacePart = _surfaceParts[index];
                var surfaceRenderer = surfacePart.ScrapePart.GetComponent<MeshRenderer>();

                _surfaceCreatedParts.Add(index, new List<SurfaceScrapeCreatePart>
                {
                    new SurfaceScrapeCreatePart
                    {
                        Mesh = surfacePart.ScrapePart.Mesh,
                        Renderer = surfaceRenderer,
                        Type = SurfaceScrapePartType.Road,
                        ClipFrom = surfacePart.ScrapePart.Mesh.clipFrom,
                        ClipTo = surfacePart.ScrapePart.Mesh.clipTo
                    }
                });
            }
        }

        protected virtual void LateUpdate()
        {
            if (IsScraping)
            {
                var currentSurface = _currentSurfaceCreatedParts[_currentSurfaceCreatedPartIndex];
                var nextSurface = _currentSurfaceCreatedParts[_currentSurfaceCreatedPartIndex + 1];

                Vector3 tipPosition = Spatula.Instance.TipPosition;
                tipPosition += GameplayController.Instance.Follower.transform.forward * .5f;

                Spline.Project(_tmpSplineSample, tipPosition);

                if (_tmpSplineSample.percent > currentSurface.ClipTo)
                {
                    currentSurface.SetClipTo(_tmpSplineSample.percent);
                    nextSurface.SetClipFrom(_tmpSplineSample.percent);
                }

                if (nextSurface.ClipFrom >= nextSurface.ClipTo)
                    nextSurface.Renderer.enabled = false;
            }
        }

        protected virtual int GetSurfacePartIndex(SurfaceScrapePart surfacePart)
        {
            for (int index = 0; index < _surfaceParts.Count; index ++)
            {
                if (_surfaceParts[index].ScrapePart == surfacePart)
                    return index;
            }

            return 0;
        }

        public virtual void ScrapeStarted(double percent, SurfaceScrapePart surfacePart)
        {
            _currentSurfacePartIndex = GetSurfacePartIndex(surfacePart);

            var scrapePart = GetPartIndex(percent);
            if (scrapePart < 0f) return;

            IsScraping = true;

            _currentSurfaceCreatedPartIndex = scrapePart;
        }

        public virtual void ScrapeStopped()
        {
            IsScraping = false;
        }

        public virtual void OnReachedRoadEnd()
        {
            if (_currentSurfaceCreatedPartIndex > 0)
                _currentSurfaceCreatedParts[_currentSurfaceCreatedPartIndex + 1].Renderer.enabled = false;
        }

        protected virtual int GetPartIndex(double percent)
        {
            int surfaceIndex = -1;
            for (int index = 0; index < _currentSurfaceCreatedParts.Count; index++)
            {
                var surface = _currentSurfaceCreatedParts[index];
                if (percent >= surface.ClipFrom && percent <= surface.ClipTo)
                {
                    surfaceIndex = index;
                    break;
                }
            }

            if (surfaceIndex >= 0)
            {
                var currentSetting = _surfaceParts[_currentSurfacePartIndex];

                var currentSurface = _currentSurfaceCreatedParts[surfaceIndex];
                var currentPartClipTo = currentSurface.ClipTo;
                if (currentSurface.Type == SurfaceScrapePartType.Road || percent > currentSurface.ClipTo)
                    currentSurface.SetClipTo(percent);
                // currentSurface.Mesh.clipTo = percent;

                // If spatula stuck on road, create a new road and scrape road.
                if (currentSurface.Type == SurfaceScrapePartType.Road)
                {
                    surfaceIndex++;

                    // Add scrape part
                    var scrapePart = Instantiate(SurfaceScrapePartHiddenPrefab ?? GameplayController.Instance.SurfaceScrapePartHiddenPrefab, transform.parent);
                    scrapePart.gameObject.name = $"{gameObject.name} + Part_Scrape_{_currentSurfaceCreatedParts.Count + 1}";
                    scrapePart.spline = Spline;
                    var scrapePartChannel = scrapePart.GetChannel(0).NextMesh();
                    scrapePartChannel.scale = new Vector3(currentSetting.Scale.x, SurfaceScrapeScale, currentSetting.Scale.z);
                    scrapePartChannel.offset = new Vector3(currentSetting.Offset.x, currentSetting.Offset.y - Mathf.Sign(currentSetting.Offset.y) * SurfaceScrapeScale * .5f, currentSetting.Offset.z);

                    // scrapePart.SetClipRange(percent, percent);
                    var newRoadPartScrapable = new SurfaceScrapeCreatePart
                    {
                        Mesh = scrapePart
                        ,
                        Renderer = scrapePart.GetComponent<MeshRenderer>()
                        ,
                        Type = SurfaceScrapePartType.Scraped
                    };
                    newRoadPartScrapable.SetClipRange(percent, percent);
                    _currentSurfaceCreatedParts.Insert(surfaceIndex, newRoadPartScrapable);

                    // Add road
                    var roadPart = Instantiate(SurfaceScrapePartPrefab ?? GameplayController.Instance.SurfaceScrapePartPrefab, transform.parent);
                    roadPart.gameObject.name = $"{gameObject.name} + Part_Road_{_currentSurfaceCreatedParts.Count + 1}";
                    roadPart.spline = Spline;
                    var roadPartChannel = roadPart.GetChannel(0).NextMesh();
                    roadPartChannel.scale = currentSetting.Scale;
                    roadPartChannel.offset = currentSetting.Offset;

                    var newRoadPart = new SurfaceScrapeCreatePart
                    {
                        Mesh = roadPart
                        ,
                        Renderer = roadPart.GetComponent<MeshRenderer>()
                        ,
                        Type = SurfaceScrapePartType.Road
                    };
                    newRoadPart.SetClipRange(percent, currentPartClipTo);
                    // roadPart.SetClipRange(percent, currentPartClipTo);
                    _currentSurfaceCreatedParts.Insert(surfaceIndex + 1, newRoadPart);
                }
            }


            return surfaceIndex;
        }
    }
}