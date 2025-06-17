using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundMixer.Core.Objects
{
    public class Scene
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public List<Mood> Moods { get; set; }
        public List<Sound> Sounds { get; set; }

        public Scene(string name)
        {
            Name = name;
            Moods = new List<Mood>();
            Sounds = new List<Sound>();
        }
    }

    public class SceneEqualityComparer : IEqualityComparer<Scene>
    {
        public bool Equals(Scene? x, Scene? y)
        {
            if (x == null || y == null) return false;
            return x?.Name == y?.Name;
        }

        public int GetHashCode([DisallowNull] Scene obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
