using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
    public partial class FormMain : Form, ILoggerNotifier
    {
        private UserFacade m_UserFacade;    
        private List<ListBox> m_ListBoxPosts;
        private List<string> m_BlockedWords;
        private volatile List<Photo> m_Photos;
        private List<PictureBox> m_PictureBoxIcons;
        private List<Label> m_DynamicLabels;
        private List<RadioButton> m_RadioButtonsListControl;
        private ListControlAdapter m_ListControlGroups;
        private ListControlAdapter m_ListControlBooks;
        private ListControlAdapter m_ListControlShows;
        private ListControlAdapter m_ListControlMovies;
        private ListControlAdapter m_ListControlMusic;
        private ListControlAdapter m_ListControlSports;
        private List<ListControlAdapter> m_ListControlAdapters;
        private List<EventHandler> m_EventHandlersTabPagesView;
        private List<PictureBox> m_PagesPictureBoxes;
        private ILogger m_FileLogger;
        private ILogger m_ConsoleLogger;
        public event Action<string> m_ReportLoggers;

        public FormMain()
        {
            InitializeComponent();
            m_UserFacade = new UserFacade();
            FacebookWrapper.FacebookService.s_CollectionLimit = 100;
            m_ListBoxPosts = new List<ListBox>(new ListBox[] { listBoxPost1, listBoxPost2, listBoxPost3 });
            m_BlockedWords = new List<string>();
            m_Photos = new List<Photo>();
            m_PictureBoxIcons = new List<PictureBox>() { pictureBoxWorkPlace, pictureBoxEdcuation, pictureBoxCity,
                                                         pictureBoxHomeTown, pictureBoxStatus, pictureBoxEmail };
            m_DynamicLabels = new List<Label>() { labelWorkPlaceValue, labelEducationValue, labelCityValue,
                                                        labelHomeTownValue, labelStatusValue, labelEmailAddressValue, labelLoggedInUserName };
            m_RadioButtonsListControl = new List<RadioButton> { radioButtonListBox, radioButtonComboBox };
            m_ListControlAdapters = new List<ListControlAdapter> { m_ListControlGroups, m_ListControlBooks, m_ListControlShows, m_ListControlMovies,
                                                         m_ListControlMusic, m_ListControlSports };
            m_EventHandlersTabPagesView = new List<EventHandler> {listControlGroups_SelectedValueChanged, listControlBooks_SelectedValueChanged, listControlShows_SelectedValueChanged,
                                                         listControlMovies_SelectedValueChanged, listControlMusic_SelectedValueChanged, listControlSports_SelectedValueChanged };
            m_PagesPictureBoxes = new List<PictureBox> { pictureBoxGroups, pictureBoxMusic, pictureBoxSports,
                                                         pictureBoxBooks, pictureBoxShows, pictureBoxMovies};
            m_FileLogger = new FileLogger(this);
            m_ConsoleLogger = new ConsoleLogger(this);
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            ApplicationSettings.Instance.LastWindowState = this.WindowState;
            ApplicationSettings.Instance.LastWindowLocation = this.Location;
            ApplicationSettings.Instance.AutoLogin = this.checkBoxRememberMe.Checked;
            ApplicationSettings.Instance.Save();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.WindowState = ApplicationSettings.Instance.LastWindowState;
            this.Location = ApplicationSettings.Instance.LastWindowLocation;
            this.checkBoxRememberMe.Checked = ApplicationSettings.Instance.AutoLogin;
            this.Width = 1000;
            this.Height = 700;

            if (ApplicationSettings.Instance.AutoLogin)
            {
                new Thread(this.autoLogin).Start();
            }
        }

        private void autoLogin()
        {
            try
            {
                m_UserFacade.AutoLogin();
                fetchUserInfo();
                buttonLogin.Invoke(new Action(() =>
                    buttonLogin.Text = $"Logged in as {m_UserFacade.Name}"));
            }
            catch
            {
                /// this is probably because of an AccessToken expiration:
                this.Invoke((Action)loginAndInit);
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            new Thread(loginAndInit).Start();
        }

        private void loginAndInit()
        {
            bool isLoggedIn = m_UserFacade.loginAndInit();

            if (isLoggedIn)
            {
                m_ReportLoggers?.Invoke("User logged in.");
                fetchUserInfo();
                buttonLogin.Invoke(new Action(() =>
                    buttonLogin.Text = $"Logged in as {m_UserFacade.Name}"));
            }
            else
            {
                MessageBox.Show(m_UserFacade.LoginResultErrorMessage, "Login Failed");
            }
        }

        private void fetchUserInfo()
        {
            pictureBoxProfile.LoadAsync(m_UserFacade.PictureNormalURL);
            foreach (Album album in m_UserFacade.Albums)
            {
                if (album.Name.Equals("Cover photos"))
                {
                    pictureBoxCoverPhoto.LoadAsync(album.PictureAlbumURL);
                }
            }

            foreach (RadioButton radioButton in m_RadioButtonsListControl)
            {
                radioButton.Invoke(new Action(() => radioButton.Visible = true));
            }

            buttonChooseListControl.Invoke(new Action(() => buttonChooseListControl.Visible = true));
            new Thread(fetchAboutAttributes).Start();
            new Thread(fetchBirthdays).Start();
            new Thread(fetchFriends).Start();
            new Thread(fetchPosts).Start();
            new Thread(fetchFeed).Start();
            new Thread(fetchPhotos).Start();
        }

        private void fetchFeed()
        {
            pictureBoxProfilePicture.LoadAsync(m_UserFacade.PictureNormalURL);
            labelLoggedInUserName.Text = m_UserFacade.Name;
            labelFeedPostStatus.Text += String.Format("{0}?", m_UserFacade.Name);
            fetchFriendsPosts();
            fetchUpcomingEvents();
        }

        private void fetchUpcomingEvents()
        {
            listBoxUpcomingEvents.Invoke(new Action(() => listBoxUpcomingEvents.Items.Clear()));
            listBoxUpcomingEvents.Invoke(new Action(() => listBoxUpcomingEvents.DisplayMember = "Name"));
            foreach (Event attendingEvent in m_UserFacade.Events)
            {
                if (m_UserFacade.IsContainedInFacebookCollection(attendingEvent.AttendingUsers) &&
                    attendingEvent.StartTime >= DateTime.Now)
                {
                    listBoxUpcomingEvents.Invoke(new Action(() => listBoxUpcomingEvents.Items.Add(attendingEvent.Name)));
                }
            }

            if (listBoxUpcomingEvents.Items.Count == 0)
            {
                listBoxUpcomingEvents.Invoke(new Action(() => listBoxUpcomingEvents.Items.Add("No upcoming events")));
                listBoxUpcomingEvents.Invoke(new Action(() => listBoxUpcomingEvents.SelectionMode = SelectionMode.None));
            }
        }

        private void fetchFriendsPosts()
        {
            listBoxPostsList.Invoke(new Action(() => listBoxPostsList.Items.Clear()));
            listBoxPostsList.Invoke(new Action(() => listBoxPostsList.DisplayMember = "Name"));
            foreach (User friend in m_UserFacade.Friends)
            {
                foreach (Post post in m_UserFacade.NewsFeed)
                {
                    if (post.Message != null)
                    {
                        listBoxPostsList.Invoke(new Action(() => listBoxPostsList.Items.Add(post.Message)));
                    }
                    else if (post.Caption != null)
                    {
                        listBoxPostsList.Invoke(new Action(() => listBoxPostsList.Items.Add(post.Caption)));
                    }
                    else
                    {
                        listBoxPostsList.Invoke(new Action(() => listBoxPostsList.Items.Add(string.Format("[{0}]", post.Type))));
                    }
                }
            }

            if (listBoxPostsList.Items.Count == 0)
            {
                listBoxPostsList.Invoke(new Action(() => listBoxPostsList.Items.Add("No Posts to display")));
                listBoxPostsList.Invoke(new Action(() => listBoxPostsList.SelectionMode = SelectionMode.None));
                listBoxPostsList.Invoke(new Action(() => listBoxCommentsForSpecificPost.SelectionMode = SelectionMode.None));
            }
        }

        private void fetchLikedPages(ListControlAdapter i_ListControlAdapterCategory, string i_Category, string i_NoLikedCategoryMessage)
        {
            if (i_ListControlAdapterCategory.Items != null)
            {
                i_ListControlAdapterCategory.ListControl.Invoke(new Action(() => i_ListControlAdapterCategory.Items.Clear()));
            }

            i_ListControlAdapterCategory.ListControl.Invoke(new Action(() => i_ListControlAdapterCategory.ListControl.DisplayMember = "Name"));
            foreach (Page likedPage in m_UserFacade.LikedPages)
            {
                if (likedPage.Category != null && likedPage.Category.Equals(i_Category))
                {
                    i_ListControlAdapterCategory.ListControl.Invoke(new Action(() => i_ListControlAdapterCategory.Items.Add(likedPage)));
                }
            }

            if (i_ListControlAdapterCategory.Items.Count == 0)
            {
                i_ListControlAdapterCategory.ListControl.Invoke(new Action(() => i_ListControlAdapterCategory.Items.Add(i_NoLikedCategoryMessage)));
                i_ListControlAdapterCategory.ListControl.Invoke(new Action(() => i_ListControlAdapterCategory.DisableSelection()));
            }
        }

        private void fetchFriends()
        {
            foreach (User friendUser in m_UserFacade.Friends)
            {
                listBoxFriendsList.Invoke(new Action(() => listBoxFriendsList.Items.Add(friendUser.Name)));
            }

            if (listBoxFriendsList.Items.Count == 0)
            {
                listBoxFriendsList.Invoke(new Action(() => listBoxFriendsList.Items.Add("You don't have any friends.")));
            }
        }

        private void fetchBirthdays()
        {
            foreach (User friendUser in m_UserFacade.Friends)
            {
                int friendUserBirthdayMonth = Int32.Parse(friendUser.Birthday.Substring(0, 2));
                int friendUserBirthdayDay = Int32.Parse(friendUser.Birthday.Substring(3, 2));
                if (friendUserBirthdayDay == (DateTime.Now.Date.Day) &&
                    friendUserBirthdayMonth == DateTime.Now.Date.Month)
                {
                    listBoxBirthdays.Invoke(new Action(() => listBoxBirthdays.Items.Add(friendUser.Name)));
                }
            }

            if (listBoxBirthdays.Items.Count == 0)
            {
                listBoxBirthdays.Invoke(new Action(() => listBoxBirthdays.Items.Add("No birthdays today.")));
            }
        }

        private void fetchAboutAttributes()
        {
            if (m_UserFacade.WorkExperiences != null)
            {
                labelWorkPlaceValue.Invoke(new Action(() => labelWorkPlaceValue.Text = m_UserFacade.WorkExperiences[0].Name));

            }

            if (m_UserFacade.Educations != null)
            {
                labelEducationValue.Invoke(new Action(() => labelEducationValue.Text = m_UserFacade.Educations[0].School.Name));
            }

            if (m_UserFacade.Location != null)
            {
                labelCityValue.Invoke(new Action(() => labelCityValue.Text = m_UserFacade.Location.Name));
            }

            if (m_UserFacade.Hometown != null)
            {
                labelHomeTownValue.Invoke(new Action(() => labelHomeTownValue.Text = m_UserFacade.Hometown.Name));
            }

            if (m_UserFacade.RelationshipStatus != eRelationshipStatus.None)
            {
                labelStatusValue.Invoke(new Action(() => labelStatusValue.Text = convertRelationshipStatus()));
            }

            if (m_UserFacade.Email != null)
            {
                labelEmailAddressValue.Invoke(new Action(() => labelEmailAddressValue.Text = m_UserFacade.Email));
            }
        }

        private void fetchSportPages()
        {
            if (m_ListControlSports.Items != null)
            {
                m_ListControlSports.ListControl.Invoke(new Action(() => m_ListControlSports.Items.Clear()));
            }

            m_ListControlSports.ListControl.Invoke(new Action(() => m_ListControlSports.ListControl.DisplayMember = "Name"));
            foreach (Page sportPage in m_UserFacade.FavofriteTeams)
            {
                m_ListControlSports.ListControl.Invoke(new Action(() => m_ListControlSports.Items.Add(sportPage)));
            }

            if (m_ListControlSports.Items.Count == 0)
            {
                m_ListControlSports.ListControl.Invoke(new Action(() => m_ListControlSports.Items.Add("You don't like sports")));
                m_ListControlSports.ListControl.Invoke(new Action(() => m_ListControlSports.DisableSelection()));
            }
        }

        private void fetchPhotos()
        {
            List<Photo> memoryPhotos = new List<Photo>();

            clearPictureBoxes(tabPagePhotos);
            foreach (Album album in m_UserFacade.Albums)
            {
                foreach (Photo photo in album.Photos)
                {
                    if (photo.CreatedTime.HasValue && photo.CreatedTime.Value.Month.Equals(DateTime.Now.Month) &&
                        photo.CreatedTime.Value.Day.Equals(DateTime.Now.Day))
                    {
                        memoryPhotos.Add(photo);
                    }
                    else
                    {
                        m_Photos.Add(photo);
                    }
                }
            }

            fillMemoryPhoto(memoryPhotos);
        }

        private void loadRandomPhotos()
        {
            List<int> visitedPhotoIndices = new List<int>();
            Random generator = new Random();

            while (visitedPhotoIndices.Count < 6)
            {
                int randomPhotoIndex = generator.Next(0, m_Photos.Count);
                if (!visitedPhotoIndices.Contains(randomPhotoIndex))
                {
                    visitedPhotoIndices.Add(randomPhotoIndex);
                }
            }

            fillPictureBoxes(tabPagePhotos, visitedPhotoIndices);
        }

        private void fillMemoryPhoto(List<Photo> i_MemoryPhotos)
        {
            Random generator = new Random();
            int memoryPhotoIndex = generator.Next(0, i_MemoryPhotos.Count);

            if (i_MemoryPhotos.Count > 0)
            {
                pictureBoxMemoryPhoto.LoadAsync(i_MemoryPhotos[memoryPhotoIndex].PictureNormalURL);
                if (i_MemoryPhotos[memoryPhotoIndex].CreatedTime != null)
                {
                    int yearDiffrence = DateTime.Now.Year - i_MemoryPhotos[memoryPhotoIndex].CreatedTime.Value.Year;
                    String labelMemoriesDateText = String.Format("Today {0} years ago", yearDiffrence);
                    labelMemoriesDate.Invoke(new Action(() => labelMemoriesDate.Text = labelMemoriesDateText));

                }
            }
            else
            {
                labelMemoriesDate.Invoke(new Action(() => labelMemoriesDate.Text = "No memories for today"));
            }
        }

        private void fillPictureBoxes(Control i_Parent, List<int> i_VisitedPhotoIndices)
        {
            int i = 0;

            foreach (Control child in i_Parent.Controls)
            {
                if (child is PictureBox)
                {
                    ((PictureBox)child).LoadAsync(m_Photos[i_VisitedPhotoIndices[i]].PictureNormalURL);
                    i++;
                }
            }
        }

        private void clearPictureBoxes(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is PictureBox)
                {
                    ((PictureBox)child).Image = null;
                }
                else
                {
                    clearPictureBoxes(child);
                }
            }
        }

        private void fetchPosts()
        {
            try
            {
                m_UserFacade.FetchPosts();
            }
            catch
            {
                MessageBox.Show("Failed to fetch posts!");
            }

            IEnumerator<Post> postsIterator = m_UserFacade.Posts.GetEnumerator();

            clearListBoxesPosts();
            if (m_UserFacade.Posts != null)
            {
                for (int i = 0; i < m_ListBoxPosts.Count; i++)
                {
                    if (!postsIterator.MoveNext())
                    {
                        break;
                    }
                    Post post = postsIterator.Current;
                    if (post.Message != null)
                    {
                        m_ListBoxPosts[i].Invoke(new Action(() => m_ListBoxPosts[i].Items.Add(post.Message)));
                    }
                    else if (post.Caption != null)
                    {
                        m_ListBoxPosts[i].Invoke(new Action(() => m_ListBoxPosts[i].Items.Add(post.Caption)));
                    }
                    else
                    {
                        m_ListBoxPosts[i].Invoke(new Action(() => m_ListBoxPosts[i].Items.Add(string.Format("[{0}]", post.Type))));
                    }
                    handleEmptyListBoxPosts(m_ListBoxPosts[i]);
                }
            }
            postsIterator.Dispose();
        }

        private void handleEmptyListBoxPosts(ListBox i_ListBoxPost)
        {
            if (i_ListBoxPost.Items.Count == 0)
            {
                i_ListBoxPost.Invoke(new Action(() => i_ListBoxPost.Items.Add("You have no posts")));
                i_ListBoxPost.Invoke(new Action(() => i_ListBoxPost.SelectionMode = SelectionMode.None));
            }
        }

        // sorts user posts by number of comments in ascending or descending order.
        // throws exception since we don't have permissions from facebook to reach posts' comments.
        private void sortPostsByComment(Comparison<Post> i_MethodToSortBy)
        {
            if (m_UserFacade.Posts == null)
            {
                MessageBox.Show("You have to wait until all posts are fetched before sorting!");
                return;
            }

            List<Post> sortedPostsByComments = new List<Post>();
            int count = 0;
            PostsSorter postsSorter = new PostsSorter(i_MethodToSortBy);

            foreach (Post post in m_UserFacade.Posts)
            {
                sortedPostsByComments.Add(post);
                count++;
                if (count > 10)
                {
                    break;
                }
            }

            try
            {
                // before using strategy pattern
                //if (i_SortOrder == "Ascending")
                //{
                //    sortedPostsByComments.Sort((x, y) => x.Comments.Count.CompareTo(y.Comments.Count));
                //}
                //else if (i_SortOrder == "Decending")
                //{
                //    sortedPostsByComments.Sort((x, y) => y.Comments.Count.CompareTo(x.Comments.Count));
                //}

                // after using strategy pattern
                postsSorter.Sort(sortedPostsByComments);
            }
            catch
            {
                MessageBox.Show("You don't have permissions for your posts' comments.");
            }

            clearListBoxesPosts();
            for (int i = 0; i < m_ListBoxPosts.Count; i++)
            {
                Post post = sortedPostsByComments.ElementAt(i);
                if (post.Message != null)
                {
                    m_ListBoxPosts[i].Items.Add(post.Message);

                }
                else if (post.Caption != null)
                {
                    m_ListBoxPosts[i].Items.Add(post.Caption);
                }
                else
                {
                    m_ListBoxPosts[i].Items.Add(string.Format("[{0}]", post.Type));
                }
                if (m_ListBoxPosts[i].Items.Count == 0)
                {
                    m_ListBoxPosts[i].Items.Add("You have no posts");
                    m_ListBoxPosts[i].SelectionMode = SelectionMode.None;
                }
            }

            setBlockedWordsAfterFilter();
        }

        private void setBlockedWordsAfterFilter()
        {
            foreach (string blockedWord in m_BlockedWords)
            {
                censorBlockedWord(blockedWord);
            }
        }

        private void filterPosts(Predicate<Post> i_FilterPrediacte)
        {
            if (m_UserFacade.Posts == null)
            {
                MessageBox.Show("You have to wait until all posts are fetched before filtering!");
                return;
            }

            m_UserFacade.Posts.FilterPredicate = i_FilterPrediacte;
            int currListBoxPostIndex = 0;

            clearListBoxesPosts();
            foreach (Post post in m_UserFacade.Posts)
            {
                if (currListBoxPostIndex == 3)
                {
                    break;
                }

                if (post.Message != null)
                {
                    m_ListBoxPosts[currListBoxPostIndex].Items.Add(post.Message);
                }
                else if (post.Caption != null)
                {
                    m_ListBoxPosts[currListBoxPostIndex].Items.Add(post.Caption);
                }
                else
                {
                    m_ListBoxPosts[currListBoxPostIndex].Items.Add(string.Format("[{0}]", post.Type));
                }
                currListBoxPostIndex++;
            }
            setBlockedWordsAfterFilter();
            foreach (ListBox listBoxPosts in m_ListBoxPosts) 
            {
                handleEmptyListBoxPosts(listBoxPosts);
            }
        }

        private void clearListBoxesPosts()
        {
            foreach (ListBox listBoxPost in m_ListBoxPosts)
            {
                listBoxPost.Invoke(new Action(() => listBoxPost.Items.Clear()));
            }
        }

        private void fetchGroups()
        {
            if (m_ListControlGroups.Items != null)
            {
                m_ListControlGroups.ListControl.Invoke(new Action(() => m_ListControlGroups.Items.Clear()));
            }

            m_ListControlGroups.ListControl.Invoke(new Action(() => m_ListControlGroups.ListControl.DisplayMember = "Name"));
            try
            {
                foreach (Group group in m_UserFacade.Groups)
                {
                    m_ListControlGroups.ListControl.Invoke(new Action(() => m_ListControlGroups.Items.Add(group)));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (m_ListControlGroups.Items.Count == 0)
            {
                m_ListControlGroups.ListControl.Invoke(new Action(() => m_ListControlGroups.Items.Add("You aren't member in groups")));
                m_ListControlGroups.ListControl.Invoke(new Action(() => m_ListControlGroups.DisableSelection()));
            }
        }

        private string convertRelationshipStatus()
        {
            switch (m_UserFacade.RelationshipStatus)
            {
                case eRelationshipStatus.Single:
                    return "Single";
                case eRelationshipStatus.InARelationship:
                    return "In A Relationship";
                case eRelationshipStatus.Enagaged:
                    return "Enagaged";
                case eRelationshipStatus.Married:
                    return "Married";
                case eRelationshipStatus.ItsComplicated:
                    return "Its Complicated";
                case eRelationshipStatus.InAnOpenRelationship:
                    return "In An Open Relationship";
                case eRelationshipStatus.Widowed:
                    return "Widowed";
                case eRelationshipStatus.Separated:
                    return "Separated";
                case eRelationshipStatus.Divorced:
                    return "Divorced";
                case eRelationshipStatus.InACivilUnion:
                    return "In A Civil Union";
                case eRelationshipStatus.InADomesticPartnership:
                    return "In A Domestic Partnership";
            }

            return null;
        }
        private void buttonLogout_Click(object sender, EventArgs e)
        {

        }

        private void clearAllAfterLogout()
        {
            foreach (Control control in this.Controls)
            {
                if (control!=null && control is TabControl)
                {
                    foreach (Control subControl in (control as TabControl).TabPages)
                    {
                        foreach (Control subSubControl in (subControl as TabPage).Controls)
                        {
                            if (subControl != null)
                            {
                                if (subSubControl is PictureBox && !m_PictureBoxIcons.Contains(subSubControl))
                                {
                                    (subSubControl as PictureBox).Image = null;
                                }
                                else if (subSubControl is ListBox)
                                {
                                    (subSubControl as ListBox).Items.Clear();
                                }
                                else if (subSubControl is ComboBox)
                                {
                                    (subSubControl as ComboBox).Items.Clear();
                                }
                                else if (subSubControl is TextBox)
                                {
                                    (subSubControl as TextBox).Text = "";
                                }
                                else if (subSubControl is DataGridView)
                                {
                                    (subSubControl as DataGridView).DataSource = null;
                                }
                                else if (m_DynamicLabels.Contains(subSubControl))
                                {
                                    (subSubControl as Label).Text = "";
                                }
                            }
                        }
                    }
                }
            }

            foreach (ListControlAdapter adapter in m_ListControlAdapters)
            {
                if (adapter != null && this.tabPagePages != null && this.tabPagePages.Controls != null)
                {
                    this.tabPagePages.Controls.Remove(adapter.ListControl);
                }
            }
        }

        private void labelBirthdays_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void labelWorkPlace_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBoxMusic_Click(object sender, EventArgs e)
        {

        }

        private void listControlMusic_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_ListControlMusic.SelectedItem != null)
            {
                Page selectedMusic = m_ListControlMusic.SelectedItem as Page;
                pictureBoxMusic.LoadAsync(selectedMusic.PictureNormalURL);
            }
        }

        private void buttonLogout_Click_1(object sender, EventArgs e)
        {
            m_UserFacade.Logout();
            buttonLogin.Text = "Login";
            try
            {
                clearAllAfterLogout();
                m_ReportLoggers?.Invoke("User logged out.");
            }
            catch
            {
                MessageBox.Show("Failed to logout!");
            }
        }

        private void listControlSports_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_ListControlSports.SelectedItem != null)
            {
                Page selectedTeam = m_ListControlSports.SelectedItem as Page;
                pictureBoxSports.LoadAsync(selectedTeam.PictureNormalURL);
            }
        }

        private void listControlGroups_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_ListControlGroups.SelectedItem != null)
            {
                Group selectedGroup = m_ListControlGroups.SelectedItem as Group;
                pictureBoxGroups.LoadAsync(selectedGroup.PictureNormalURL);
            }
        }

        private void labelMemoriesDate_Click(object sender, EventArgs e)
        {

        }

        private void pictureBoxCoverPhoto_Click(object sender, EventArgs e)
        {

        }

        private void listBoxBirthdays_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listControlBooks_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_ListControlBooks.SelectedItem != null)
            {
                Page selectedBook = m_ListControlBooks.SelectedItem as Page;
                pictureBoxBooks.LoadAsync(selectedBook.PictureNormalURL);
            }
        }

        private void listControlShows_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_ListControlShows.SelectedItem != null)
            {
                Page selectedShow = m_ListControlShows.SelectedItem as Page;
                pictureBoxShows.LoadAsync(selectedShow.PictureNormalURL);
            }
        }

        private void listControlMovies_SelectedValueChanged(object sender, EventArgs e)
        {
            if (m_ListControlMovies.SelectedItem != null)
            {
                Page selectedMovie = m_ListControlMovies.SelectedItem as Page;
                pictureBoxMovies.LoadAsync(selectedMovie.PictureNormalURL);
            }
        }

        private void listBoxPost1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            string itemText = (sender as ToolStripMenuItem).Text;
            int yearToBeFilteredBy = Int32.Parse(itemText);
            filterPosts((post) => post.CreatedTime.Value.Year == yearToBeFilteredBy);
        }

        private void toolStripMenuItemYear_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            string itemText = (sender as ToolStripMenuItem).Text;
            int yearToBeFilteredBy = Int32.Parse(itemText);
            filterPosts((post) => post.CreatedTime.Value.Year == yearToBeFilteredBy);
        }

        private void toolStripMenuItem2020_Click(object sender, EventArgs e)
        {
            string itemText = (sender as ToolStripMenuItem).Text;
            int yearToBeFilteredBy = Int32.Parse(itemText);
            filterPosts((post) => post.CreatedTime.Value.Year == yearToBeFilteredBy);
        }

        private void toolStripMenuItem2019_Click(object sender, EventArgs e)
        {
            string itemText = (sender as ToolStripMenuItem).Text;
            int yearToBeFilteredBy = Int32.Parse(itemText);
            filterPosts((post) => post.CreatedTime.Value.Year == yearToBeFilteredBy);
        }

        private void toolStripMenuItem2018_Click(object sender, EventArgs e)
        {
            string itemText = (sender as ToolStripMenuItem).Text;
            int yearToBeFilteredBy = Int32.Parse(itemText);
            filterPosts((post) => post.CreatedTime.Value.Year == yearToBeFilteredBy);
        }

        private void toolStripMenuItem2017_Click(object sender, EventArgs e)
        {
            string itemText = (sender as ToolStripMenuItem).Text;
            int yearToBeFilteredBy = Int32.Parse(itemText);
            filterPosts((post) => post.CreatedTime.Value.Year == yearToBeFilteredBy);
        }

        private void toolStripMenuItem2016_Click(object sender, EventArgs e)
        {
            string itemText = (sender as ToolStripMenuItem).Text;
            int yearToBeFilteredBy = Int32.Parse(itemText);
            Console.WriteLine("yearToBeFilteredBy");
            filterPosts((post) => post.CreatedTime.Value.Year == yearToBeFilteredBy);
        }

        private void toolStripMenuItem2015_Click(object sender, EventArgs e)
        {
            string itemText = (sender as ToolStripMenuItem).Text;
            int yearToBeFilteredBy = Int32.Parse(itemText);
            filterPosts((post) => post.CreatedTime.Value.Year == yearToBeFilteredBy);
        }

        private void toolStripMenuItem2014_Click(object sender, EventArgs e)
        {
            string itemText = (sender as ToolStripMenuItem).Text;
            int yearToBeFilteredBy = Int32.Parse(itemText);
            filterPosts((post) => post.CreatedTime.Value.Year == yearToBeFilteredBy);
        }

        private void toolStripMenuItemComments_Click(object sender, EventArgs e)
        {

        }

        private void ascendingOrderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            sortPostsByComment((x, y) => x.Comments.Count.CompareTo(y.Comments.Count));
        }

        private void decendingOrderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            sortPostsByComment((x, y) => y.Comments.Count.CompareTo(x.Comments.Count));
        }

        private void toolStripMenuItemLink_Click(object sender, EventArgs e)
        {
            filterPosts((post) => post.Type == Post.eType.link);
        }

        private void toolStripMenuItemPhoto_Click(object sender, EventArgs e)
        {
            filterPosts((post) => post.Type == Post.eType.photo);
        }

        private void toolStripMenuItemVideo_Click(object sender, EventArgs e)
        {
            filterPosts((post) => post.Type == Post.eType.video);
        }

        private void toolStripMenuItemCheckIn_Click(object sender, EventArgs e)
        {
            filterPosts((post) => post.Type == Post.eType.checkin);
        }

        private void toolStripMenuItemStatus_Click(object sender, EventArgs e)
        {
            filterPosts((post) => post.Type == Post.eType.status);
        }

        private void toolStripMenuItemSWF_Click(object sender, EventArgs e)
        {
            filterPosts((post) => post.Type == Post.eType.swf);
        }

        private void buttonPost_Click(object sender, EventArgs e)
        {
            postStatus(textBoxPostStatus);
        }

        private void postStatus(TextBox i_PostStatus)
        {
            if (!string.IsNullOrEmpty(i_PostStatus.Text))
            {
                try
                {
                    m_UserFacade.PostStatus(i_PostStatus.Text);
                    MessageBox.Show("Your Post has been added!");
                    m_ReportLoggers?.Invoke("User posted a new post.");
                }
                catch
                {
                    MessageBox.Show("You don't have permissions to post status!");
                }
            }
            else
            {
                MessageBox.Show("Please enter a post.");
            }

            i_PostStatus.Text = "";
        }

        private void buttonBlockedWord_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxBlockedWords.Text))
            {
                if (!m_BlockedWords.Contains(textBoxBlockedWords.Text))
                {
                    m_BlockedWords.Add(textBoxBlockedWords.Text);
                    censorBlockedWord(textBoxBlockedWords.Text);
                    MessageBox.Show("The word was censored");
                    m_ReportLoggers?.Invoke("User entered a new blocked word.");
                }
                else
                {
                    MessageBox.Show("You already added this word to block.");
                }
            }
            else
            {
                MessageBox.Show("Please enter a word");
            }
        }

        private void censorBlockedWord(string i_WordToBeCensored)
        {
            foreach (Control control in this.Controls)
            {
                if (control is TabControl)
                {
                    foreach (Control subControl in (control as TabControl).TabPages)
                    {
                        foreach (Control subSubControl in (subControl as TabPage).Controls)
                        {
                            if (subSubControl is ListBox)
                            {
                                ListBox listBox = (ListBox)subSubControl;
                                for (int i = 0; i < listBox.Items.Count; i++)
                                {
                                    string currentPostText = listBox.Items[i].ToString();
                                    string newText = censorBlockedWordInListBox(i_WordToBeCensored, listBox, currentPostText);
                                    listBox.Items[i] = newText;
                                }
                            }
                        }
                    }
                }
            }
        }

        private string censorBlockedWordInListBox(string i_WordToBeCensored, ListBox i_ListBoxPost, string i_CurrentPostText)
        {
            string newText = i_CurrentPostText;

            if (i_CurrentPostText.Contains(i_WordToBeCensored))
            {
                StringBuilder replacementAstrix = new StringBuilder();
                for (int i = 0; i < i_WordToBeCensored.Length; i++)
                {
                    replacementAstrix.Append("*");
                }
                newText = i_CurrentPostText.Replace(i_WordToBeCensored, replacementAstrix.ToString());
            }

            return newText;
        }

        private void textBoxBlockedWords_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBoxBlockedWords_MouseClick(object sender, MouseEventArgs e)
        {
            textBoxBlockedWords.Text = "";
        }

        private void pictureBoxProfile_Click(object sender, EventArgs e)
        {

        }

        private void buttonPostInFeed_Click(object sender, EventArgs e)
        {
            postStatus(textBoxStatusFeed);
        }

        private void listBoxPostsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonFeedLike.Click += new System.EventHandler(this.buttonFeedLike_Click);
            if (listBoxPostsList.SelectedItems.Count == 1)
            {
                Post selected = null;

                try 
                {
                    selected = m_UserFacade.GetPostByIndex(listBoxPostsList.SelectedIndex);
                }
                catch 
                {
                    MessageBox.Show("Failed to select post.");
                }

                try
                {
                    if (selected != null)
                    {
                        listBoxCommentsForSpecificPost.DataSource = selected.Comments;
                        listBoxCommentsForSpecificPost.DisplayMember = "Message";
                        if (selected.LikedBy != null && m_UserFacade.IsContainedInFacebookCollection(selected.LikedBy))
                        {
                            buttonFeedLike.BackgroundImage = Properties.Resources.UnlikeButton;
                        }
                        else
                        {
                            buttonFeedLike.BackgroundImage = Properties.Resources.LikedButton;
                        }
                    }
                    else
                    {
                        listBoxCommentsForSpecificPost.Items.Add("No comments.");
                        listBoxCommentsForSpecificPost.SelectionMode = SelectionMode.None;
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to load comments.");
                }
            }
        }

        private void buttonFeedLike_Click(object sender, EventArgs e)
        {
            Post selected = null;

            try
            {
                selected = m_UserFacade.GetPostByIndex(listBoxPostsList.SelectedIndex);
            }
            catch
            {
                MessageBox.Show("Failed to select post.");
            }

            try
            {

                if (m_UserFacade.IsContainedInFacebookCollection(selected.LikedBy))
                {
                    selected.Unlike();
                    m_ReportLoggers?.Invoke("User unliked a post.");
                }
                else
                {
                    selected.Like();
                    m_ReportLoggers?.Invoke("User liked a post.");
                }
            }
            catch
            {
                MessageBox.Show("Failed to like/unlike post.");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBoxCommentsForSpecificPost_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBoxUpcomingEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelSourceAddress.Visible = true;
            textBoxSourceAddress.Visible = true;
        }

        private void fetchEventRoute(Event i_Selected, string i_SourceAddress, Form i_FormEventDirections)
        {
            string destination = i_Selected.Location;
            StringBuilder queryAddress = new StringBuilder("https://www.google.com/maps/dir/");
            queryAddress.Append(i_SourceAddress + "/" + destination);
            foreach (Control control in i_FormEventDirections.Controls)
            {
                if (control is WebBrowser)
                {
                    (control as WebBrowser).Navigate(queryAddress.ToString());
                }
            }
        }

        private void labelSourceAddress_Click(object sender, EventArgs e)
        {

        }

        private void webBrowserEventsDirection_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void textBoxSourceAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    FormEventDirections formEventDirections = new FormEventDirections(this);
                    Event selected = m_UserFacade.Events[listBoxUpcomingEvents.SelectedIndex];
                    fetchEventRoute(selected, textBoxSourceAddress.Text, formEventDirections);
                    this.Hide();
                    formEventDirections.ShowDialog();
                    m_ReportLoggers?.Invoke("User fetched event route.");
                }
                catch
                {
                    MessageBox.Show("Failed to fetch route to event.");
                }
            }
        }

        private void buttonPlayMusic_Click(object sender, EventArgs e)
        {
            VideoSearch videoItems = new VideoSearch();
            List<YouTubeVideo> videoList = new List<YouTubeVideo>();
            List<VideoInformation> findingsList;

            if (m_ListControlMusic.Items.Count == 0)
            {
                MessageBox.Show("You don't like any artists.");
            }
            else
            {
                try
                {
                    findingsList = videoItems.SearchQuery(chosenArtistPageName(), 1);
                    System.Diagnostics.Debug.WriteLine(findingsList.Count);
                    foreach (VideoInformation item in findingsList)
                    {
                        YouTubeVideo video = new YouTubeVideo
                        {
                            Title = item.Title,
                            Author = item.Author,
                            Url = item.Url
                        };
                        byte[] imageInBytes = new WebClient().DownloadData(item.Thumbnail);
                        using (MemoryStream ms = new MemoryStream(imageInBytes))
                        {
                            video.Thumbnail = Image.FromStream(ms);
                        }

                        videoList.Add(video);
                    }
                    youTubeVideoBindingSource.DataSource = videoList;
                    m_ReportLoggers?.Invoke("User played YouTube.");
                }
                catch
                {
                    MessageBox.Show("Failed to load Youtube videos.");
                }
            }       
        }

        //We are using random in order to get a artist name to be searched by in youtube.
        //We know that we can't fetch music page data from facebook, we have tried to run the searching proccess with
        //sports pages (which we know we can fetch them) and it worked fine =)
        private string chosenArtistPageName()
        {
            Random randomdArtistToBeChosen = new Random();
            int artistIndex = randomdArtistToBeChosen.Next(0, m_ListControlMusic.Items.Count);
            string artistName = m_ListControlMusic.Items[artistIndex].ToString();

            return artistName;
        }

        private void buttonFetchPhotos_Click(object sender, EventArgs e)
        {
            if (m_Photos.Count == 0)
            {
                fetchPhotos();
            }
            loadRandomPhotos();
            m_ReportLoggers?.Invoke("User fetched random photos.");
        }

        private void listBoxPost3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void labelFeedPostStatus_Click(object sender, EventArgs e)
        {

        }

        private void textBoxSourceAddress_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonFeedLike_Click_1(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        private void youTubeVideoBindingSource_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void buttonChooseEventView_Click(object sender, EventArgs e)
        {

        }

        private void buttonChooseEventView_Click_1(object sender, EventArgs e)
        {
            string listControlType = null;

            foreach (RadioButton radioButton in m_RadioButtonsListControl)
            {
                if (radioButton.Checked)
                {
                    if (radioButton == radioButtonListBox)
                    {
                        listControlType = "ListBox";
                    }
                    else if (radioButton == radioButtonComboBox)
                    {
                        listControlType = "ComboBox";
                    }
                }
            }

            createTabPagesControls(listControlType);
            foreach (RadioButton radioButton in m_RadioButtonsListControl)
            {
                radioButton.Visible = false;
            }

            buttonChooseListControl.Visible = false;
            m_ReportLoggers?.Invoke("User changed ListControl type.");
        }

        private void createTabPagesControls(string i_ListControlType)
        {
            int listControlPositionX = 30;
            int ListControlPositionY = 81;
            const int k_ListControlPositionWidth = 230;
            const int k_ListControlPositionHeight = 199;
            int listControlPositionHeightTabIndex = 43;

            // creating the list control and initialzing it on the form itself.
            for (int i = 0; i < m_ListControlAdapters.Count; i++)
            {
                if (i == 3)
                {
                    listControlPositionX = 30;
                    ListControlPositionY = 365;
                }
                m_ListControlAdapters[i] = new ListControlAdapter { ListControl = listControlFactory.CreateListControl(i_ListControlType) };
                this.tabPagePages.Controls.Add(m_ListControlAdapters[i].ListControl);
                m_ListControlAdapters[i].ListControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
                m_ListControlAdapters[i].ListControl.DisplayMember = "name";
                m_ListControlAdapters[i].ListControl.FormattingEnabled = true;
                m_ListControlAdapters[i].ItemHeight = 16;
                m_ListControlAdapters[i].ListControl.Location = new System.Drawing.Point(listControlPositionX, ListControlPositionY);
                m_ListControlAdapters[i].ListControl.Margin = new System.Windows.Forms.Padding(4);
                m_ListControlAdapters[i].ListControl.Name = "listBoxGroups";
                m_ListControlAdapters[i].ListControl.Size = new System.Drawing.Size(k_ListControlPositionWidth, k_ListControlPositionHeight);
                m_ListControlAdapters[i].ListControl.TabIndex = listControlPositionHeightTabIndex;
                listControlPositionHeightTabIndex += 2;
                m_ListControlAdapters[i].ListControl.Visible = true;
                m_ListControlAdapters[i].ListControl.SelectedValueChanged += new System.EventHandler(m_EventHandlersTabPagesView[i]);
                listControlPositionX += 274;
                m_PagesPictureBoxes[i].Visible = true;
            }

            initializeListControls();
            fillTabPagesListControls();
        }

        private void fillTabPagesListControls()
        {
            new Thread(fetchSportPages).Start();
            new Thread(fetchGroups).Start();
            new Thread(() =>
                fetchLikedPages(m_ListControlMusic, "Musician/Band", "You don't like music")).Start();
            new Thread(() =>
                fetchLikedPages(m_ListControlMovies, "Movie", "You don't like movies")).Start();
            new Thread(() =>
                fetchLikedPages(m_ListControlShows, "TV show", "You don't watch TV")).Start();
            new Thread(() =>
                fetchLikedPages(m_ListControlBooks, "Book", "You don't like to read")).Start();
        }

        private void initializeListControls()
        {
            m_ListControlGroups = m_ListControlAdapters[0];
            m_ListControlBooks = m_ListControlAdapters[1];
            m_ListControlShows = m_ListControlAdapters[2];
            m_ListControlMovies = m_ListControlAdapters[3];
            m_ListControlMusic = m_ListControlAdapters[4];
            m_ListControlSports = m_ListControlAdapters[5];
        }

        private void tabPageProfile_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
