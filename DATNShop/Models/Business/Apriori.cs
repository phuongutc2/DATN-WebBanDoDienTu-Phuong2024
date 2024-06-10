using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ShoppingOnline.Models.Business
{
    public class Apriori
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        List<string> list;
        List<string> DistinctValues;
        List<ItemSet> ItemSets;
        public Apriori(long ID)
        {
            var lstOrder = (from order in db.Orders
                           join detail in db.Order_Detail on order.ID equals detail.Order_ID
                           where detail.Product_ID == ID
                           select order).ToList();
            list = new List<string>();
            foreach(var item in lstOrder)
            {
                var str = "";
                foreach(var jtem in db.Order_Detail.Where(x => x.Order_ID == item.ID).ToList())
                {
                    str += jtem.Product_ID + " ";
                }
                list.Add(str);
                if (list.Count == 10)
                    break;
            }

            ItemSets = new List<ItemSet>();
            SetDistinctValues(list);
        }


        //Tìm tập ứng viên k-Items từ tập k-1 items
        //lenght => k , support -> độ phổ biến, Candidates-> Kêt hợp, IsFirstItemList -> Có phải là tập ứng viên đầu k?
        public ItemSet GetItemSet(int length, int support, bool Candidates = false, bool IsFirstItemList = false)
        {
            List<IEnumerable<string>> result = GetPermutations(DistinctValues, length).ToList(); //Lấy tập ứng viên đã thu gọn / kết hợp, tương ứng lenght( k ) = 1 
            List<List<string>> data = new List<List<string>>();
            foreach (var item in result)//Lấy các ứng viên ra để so sánh vs CSDL, tính độ phổ biến và độ tin cậy
            {
                data.Add(item.ToList());
            }
            ItemSet itemSet = new ItemSet();
            itemSet.Support = support;
            itemSet.Label = (Candidates ? "C" : "L") + length.ToString(); //Lưu thứ tự vòng Loop: L1, L2,.... Mục đích: load lên panel giao diện
            foreach (var item in data) //Tập ứng viên
            {
                int count = 0; //Biến đếm độ phổ biến
                foreach (var word in list) // CSDL ban đầu
                {
                    bool found = false;
                    foreach (var item2 in item)
                    {
                        if (word.Split(' ').Contains(item2))
                            found = true;
                        else
                        {
                            found = false;
                            break;
                        }

                    }
                    if (found)
                        count++;
                }

                //Candidates => Có tập kết hợp != rỗng
                //IsFirstItemList => Các tập ứng viên kết hợp lần đầu sau khi đã distinct (thu gọn)
                //Số lượng(count) Lớn hơn độ phổ biến đã cài sẵn (support)
                if ((Candidates && count > 0) || IsFirstItemList || count >= support)
                {
                    itemSet.Add(item, count);//Lưu tập để show ra panel
                    ItemSets.Add(itemSet); //Lưu tập ứng viên đã kết hợp cùng với số lượng tương ứng
                }
            }
            return itemSet;
        }


        //Thu gọn tập ứng viên và tính toán số lượng tương ứng
        public void SetDistinctValues(List<string> values)
        {
            List<string> data = new List<string>();
            foreach (var item in values)
            {
                var row = item.Split(' ');
                foreach (var item2 in row)
                {
                    if (string.IsNullOrWhiteSpace(item2)) continue;
                    if (!data.Contains(item2))
                        data.Add(item2);
                }
            }
            DistinctValues = new List<string>();
            DistinctValues.AddRange(data.OrderBy(a => a).ToList());
        }


        //Đệ quy kết hợp các tập ứng viên
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> items, int count)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (count == 1)
                    yield return new T[] { item };
                else
                {
                    foreach (var result in GetPermutations(items.Skip(i + 1), count - 1))
                        yield return new T[] { item }.Concat(result);
                }

                ++i;
            }
        }

        //Các luật kết hợp và tính toán độ tin cậy
        public List<AssociationRule> GetRules(ItemSet items)
        {
            List<AssociationRule> rules = new List<AssociationRule>();
            foreach (var item in items)
            {
                foreach (var set in item.Key)
                {
                    //Lưu tập kết hợp từ 
                    rules.Add(GetSingleRule(set, item));
                    if (item.Key.Count > 2) //Nếu tập kết hợp có từ 3 ứng viên trở lên
                        rules.Add(GetSingleRule(item.Key.ToDisplay(exclude: set), item));
                }
            }

            return rules.OrderByDescending(a => a.Support).ThenByDescending(a => a.Confidance).ToList();
        }

        private AssociationRule GetSingleRule(string set, KeyValuePair<List<string>, int> item)
        {
            var setItems = set.Split(',');
            for (int i = 0; i < setItems.Count(); i++)
            {
                setItems[i] = setItems[i].Trim();
            }
            AssociationRule rule = new AssociationRule();

            #region Show tập kết hợp ra
            StringBuilder sb = new StringBuilder();
            sb.Append(set).Append(" => ");
            List<string> list = new List<string>();
            foreach (var set2 in item.Key)
            {
                if (setItems.Contains(set2)) continue;
                list.Add(set2);
            }
            sb.Append(list.ToDisplay());
            rule.Label = sb.ToString();
            #endregion

            int totalSet = 0;

            //Tính totalSet
            foreach (var first in ItemSets)
            {
                var myItem = first.Keys.Where(a => a.ToDisplay() == set);
                if (myItem.Count() > 0)
                {
                    first.TryGetValue(myItem.FirstOrDefault(), out totalSet);
                    break;
                }
            }
            rule.Confidance = Math.Round(((double)item.Value / totalSet) * 100, 2);
            rule.Support = Math.Round(((double)item.Value / this.list.Count) * 100, 2);
            return rule;
        }
    }
}