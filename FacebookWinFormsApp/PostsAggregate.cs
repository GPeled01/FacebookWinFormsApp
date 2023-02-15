using FacebookWrapper;
using FacebookWrapper.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicFacebookFeatures
{
    public class PostsAggregate : IEnumerable<Post>
    {
        private readonly FacebookObjectCollection<Post> m_Posts;

        public Predicate<Post> FilterPredicate { get; set; }

        public PostsAggregate(FacebookObjectCollection<Post> i_Posts)
        {
            m_Posts = i_Posts;
        }

        public IEnumerator<Post> GetEnumerator()
        {
            foreach (Post post in m_Posts)
            {
                if (FilterPredicate == null || FilterPredicate.Invoke(post)) 
                {
                    yield return post;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
