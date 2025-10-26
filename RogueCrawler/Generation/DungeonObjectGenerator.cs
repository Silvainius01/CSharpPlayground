using System;
using System.Collections.Generic;
using System.Text;

namespace RogueCrawler
{
    /// <summary>
    /// Base class for <see cref="IDungeonObject"/> generators.
    /// </summary>
    /// <typeparam name="TObject">The <see cref="IDungeonObject"/> type being generated</typeparam>
    /// <typeparam name="TParams">The <see cref="BaseGenerationParameters"/> type for the objects generated.</typeparam>
    abstract class BaseDungeonObjectGenerator<TObject, TParams> 
        where TObject : class, IDungeonObject 
        where TParams : BaseGenerationParameters
    {
        static int nextId = 0;
        protected static int NextId { get => ++nextId; }

        public abstract TObject Generate(TParams oParams);
        protected void ResetId() { nextId = 0; }

        protected float GetQuality(ItemGenerationParameters iParams)
            => iParams.QualityOverride < 0.0f
                ? DungeonGenerator.GetItemQuality(iParams.Quality, iParams.QualityBias)
                : iParams.QualityOverride;
    }

    /// <summary>
    /// Base class for <see cref="IDungeonObject"/> generators that also require <see cref="DungeonObjectManager{T}"/>'s
    /// </summary>
    /// <typeparam name="TObject">The <see cref="IDungeonObject"/> type being generated</typeparam>
    /// <typeparam name="TParams">The <see cref="BaseGenerationParameters"/> type for <typeparamref name="TObject"/>.</typeparam>
    /// <typeparam name="TManager">The <see cref="DungeonObjectManager{T}"/> type for <typeparamref name="TObject"/>.</typeparam>
    abstract class DungeonObjectGenerator<TObject, TParams, TManager> : BaseDungeonObjectGenerator<TObject, TParams>
        where TObject : class, IDungeonObject
        where TParams : BaseGenerationParameters
        where TManager : DungeonObjectManager<TObject>
    {
        public abstract TManager GenerateObjects(DungeonGenerationParameters oParams, DungeonRoomManager roomManager);
    }
}
