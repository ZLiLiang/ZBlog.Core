namespace ZBlog.Core.Common.Helper
{
    /// <summary>
    /// 泛型递归求树形结构
    /// </summary>
    public static class RecursionHelper
    {
        public static void LoopToAppendChildren(List<PermissionTree> all, PermissionTree curItem, long pid, bool needbtn)
        {
            var subItems = all.Where(pt => pt.PId == curItem.Value).ToList();
            var btnItems = subItems.Where(pt => pt.IsBtn == true).ToList();
            if (subItems.Count > 0)
            {
                curItem.Btns = [.. btnItems];
            }
            else
            {
                curItem.Btns = null;
            }

            if (!needbtn)
            {
                subItems = subItems.Where(pt => pt.IsBtn == false).ToList();
            }

            if (subItems.Count > 0)
            {
                curItem.Children = [.. subItems];
            }
            else
            {
                curItem.Children = null;
            }

            if (curItem.IsBtn)
            {
                //curItem.label += "按钮";
            }

            foreach (var subItem in subItems)
            {
                if (subItem.Value == pid && pid > 0)
                {
                    //subItem.disabled = true;//禁用当前节点
                }
                LoopToAppendChildren(all, subItem, pid, needbtn);
            }
        }

        public static void LoopToAppendChildren(List<DepartmentTree> all, DepartmentTree curItem, long pid)
        {
            var subItems = all.Where(dt => dt.PId == curItem.Value).ToList();
            if (subItems.Count > 0)
            {
                curItem.Children = [.. subItems];
            }
            else
            {
                curItem.Children = null;
            }

            foreach (var subItem in subItems)
            {
                if (subItem.Value == pid && pid > 0)
                {
                    //subItem.disabled = true;//禁用当前节点
                }
                LoopToAppendChildren(all, subItem, pid);
            }
        }

        public static void LoopNaviBarAppendChildren(List<NavigationBar> all, NavigationBar curItem)
        {
            var subItems = all.Where(nb => nb.PId == curItem.Id).ToList();

            if (subItems.Count > 0)
            {
                curItem.Children = [.. subItems];
            }
            else
            {
                curItem.Children = null;
            }


            foreach (var subItem in subItems)
            {
                LoopNaviBarAppendChildren(all, subItem);
            }
        }

        public static void LoopToAppendChildrenT<T>(List<T> all, T curItem, string parentIdName = "PId", string idName = "Value", string childrenName = "Children")
        {
            var subItems = all.Where(ee => ee.GetType().GetProperty(parentIdName).GetValue(ee, null).ToString() == curItem.GetType().GetProperty(idName).GetValue(curItem, null).ToString()).ToList();

            if (subItems.Count > 0) curItem.GetType().GetField(childrenName).SetValue(curItem, subItems);
            foreach (var subItem in subItems)
            {
                LoopToAppendChildrenT(all, subItem);
            }
        }

        /// <summary>
        /// 将父子级数据结构转换为普通list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> TreeToList<T>(List<T> list, Action<T, T, List<T>> action = null)
        {
            List<T> results = new List<T>();
            foreach (var item in list)
            {
                results.Add(item);
                OperationChildData(results, item, action);
            }

            return results;
        }

        /// <summary>
        /// 递归子级数据
        /// </summary>
        /// <param name="allList">树形列表数据</param>
        /// <param name="item">Item</param>
        public static void OperationChildData<T>(List<T> allList, T item, Action<T, T, List<T>> action)
        {
            dynamic dynItem = item;
            if (dynItem.Children == null) return;
            if (dynItem.Children.Count <= 0) return;
            allList.AddRange(dynItem.Children);
            foreach (var subItem in dynItem.Children)
            {
                action?.Invoke(item, subItem, allList);
                OperationChildData(allList, subItem, action);
            }
        }

    }

    #region 树形结构所需的模型

    public class PermissionTree
    {
        public long Value { get; set; }
        public long PId { get; set; }
        public string Label { get; set; }
        public int Order { get; set; }
        public bool IsBtn { get; set; }
        public bool Disabled { get; set; }
        public List<PermissionTree> Children { get; set; }
        public List<PermissionTree> Btns { get; set; }
    }

    public class DepartmentTree
    {
        public long Value { get; set; }
        public long PId { get; set; }
        public string Label { get; set; }
        public int Order { get; set; }
        public bool Disabled { get; set; }
        public List<DepartmentTree> Children { get; set; }
    }

    public class NavigationBar
    {
        public long Id { get; set; }
        public long PId { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public bool IsHide { get; set; } = false;
        public bool IsButton { get; set; } = false;
        public string Path { get; set; }
        public string Func { get; set; }
        public string IconCls { get; set; }
        public NavigationBarMeta Meta { get; set; }
        public List<NavigationBar> Children { get; set; }
    }

    public class NavigationBarMeta
    {
        public string Title { get; set; }
        public bool RequireAuth { get; set; } = true;
        public bool NoTabPage { get; set; } = false;
        public bool KeepAlive { get; set; } = false;
    }


    public class NavigationBarPro
    {
        public long Id { get; set; }
        public long ParentId { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public bool IsHide { get; set; } = false;
        public bool IsButton { get; set; } = false;
        public string Path { get; set; }
        public string Component { get; set; }
        public string Func { get; set; }
        public string IconCls { get; set; }
        public NavigationBarMetaPro Meta { get; set; }
    }

    public class NavigationBarMetaPro
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public bool Show { get; set; } = false;
    }

    #endregion
}
