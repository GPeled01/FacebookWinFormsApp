using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FacebookWrapper.ObjectModel;
using FacebookWrapper;
using static FacebookWrapper.ObjectModel.User;
using YoutubeSearch;
using System.Net;
using System.IO;
using BasicFacebookFeatures.WithSingltonAppSettings;
using System.Threading;

namespace BasicFacebookFeatures
{
    public class UserFacade
    {
        private User m_LoggedInUser;
        private LoginResult m_LoginResult;

        public string Name 
        { 
            get => m_LoggedInUser.Name;  
        }

        public string LoginResultErrorMessage 
        {
            get => m_LoginResult.ErrorMessage;
        }

        public string PictureNormalURL 
        { 
            get => m_LoggedInUser.PictureNormalURL;
        }

        public FacebookObjectCollection<Album> Albums 
        {
            get => m_LoggedInUser.Albums;
        }

        public FacebookObjectCollection<Event> Events 
        {
            get => m_LoggedInUser.Events;
        }

        public FacebookObjectCollection<User> Friends 
        { 
            get => m_LoggedInUser.Friends; 
        }

        public FacebookObjectCollection<Post> NewsFeed
        {
            get => m_LoggedInUser.NewsFeed;
        }

        public FacebookObjectCollection<Page> LikedPages
        {
            get => m_LoggedInUser.LikedPages;
        }

        public WorkExperience[] WorkExperiences 
        {
            get => m_LoggedInUser.WorkExperiences;
        }

        public Education[] Educations
        {
            get => m_LoggedInUser.Educations;
        }

        public City Location 
        {
            get => m_LoggedInUser.Location;
        }

        public City Hometown
        {
            get => m_LoggedInUser.Hometown;
        }

        public eRelationshipStatus? RelationshipStatus
        {
            get => m_LoggedInUser.RelationshipStatus;
        }

        public string Email
        {
            get => m_LoggedInUser.Email;
        }

        public Page[] FavofriteTeams
        {
            get => m_LoggedInUser.FavofriteTeams;
        }

        public PostsAggregate Posts { get; private set; }

        public FacebookObjectCollection<Group> Groups
        {
            get => m_LoggedInUser.Groups;
        }

        public bool loginAndInit() {
            bool isLoggedIn = false;
            m_LoginResult = FacebookService.Login(
                        /// (This is Desig Patter's App ID. replace it with your own)
                        "1163986357862922",
                        /// requested permissions:
                        "email",
                        "public_profile",
                        "user_age_range",
                        "user_birthday",
                        "user_events",
                        "user_friends",
                        "user_gender",
                        "user_hometown",
                        "user_likes",
                        "user_link",
                        "user_location",
                        "user_photos",
                        "user_posts",
                        "user_videos",
                        "pages_read_user_content");

            if (!string.IsNullOrEmpty(m_LoginResult.AccessToken))
            {
                ApplicationSettings.Instance.AccessToken = m_LoginResult.AccessToken;
                m_LoggedInUser = m_LoginResult.LoggedInUser;
                isLoggedIn = true;
            }

            return isLoggedIn;
        }

        public void AutoLogin()
        {
            LoginResult result = FacebookService.Connect(ApplicationSettings.Instance.AccessToken);
            m_LoggedInUser = result.LoggedInUser;
        }

        public void Logout()
        {
            FacebookService.LogoutWithUI();
            m_LoginResult = null;
        }

        public void PostStatus(string i_Text)
        {
            m_LoggedInUser.PostStatus(i_Text);
        }

        public bool IsContainedInFacebookCollection(FacebookObjectCollection<User> i_FacebookCollection)
        {
            return i_FacebookCollection.Contains(m_LoggedInUser);
        }

        public Post GetPostByIndex(int i_Index)
        {
            return m_LoggedInUser.Posts[i_Index];
        }

        public void FetchPosts()
        {
            Posts = new PostsAggregate(m_LoggedInUser.Posts);
        }
    }
}
