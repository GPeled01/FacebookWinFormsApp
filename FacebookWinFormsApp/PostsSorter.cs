using FacebookWrapper.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicFacebookFeatures
{
    public class PostsSorter
    {
        public Comparison<Post> MethodToSortBy { get; set; }

        public PostsSorter(Comparison<Post> i_MethodToSortBy) 
        {
            MethodToSortBy = i_MethodToSortBy;
        }

        public void Sort(List<Post> i_PostsToSort)
        {
            i_PostsToSort.Sort(MethodToSortBy);
        }
    }
}
