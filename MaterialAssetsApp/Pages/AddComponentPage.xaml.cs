using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddComponentPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private readonly List<AssetComponent> _tempList;
        private readonly Action _refresh;
        private readonly int? _cardId; // nullable!

        // Режим создания карточки
        public AddComponentPage(List<AssetComponent> list, Action refresh)
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            _tempList = list;
            _refresh = refresh;
            _cardId = null; // создание карточки
        }

        // Режим редактирования карточки
        public AddComponentPage(int cardId, Action refresh)
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            _cardId = cardId; // редактирование карточки
            _refresh = refresh;
            _tempList = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название.");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Количество должно быть числом > 0.");
                return;
            }

            var comp = new AssetComponent
            {
                ComponentName = txtName.Text,
                ComponentNumber = txtNumber.Text,
                Quantity = qty,
                Notes = txtNotes.Text
            };

            // ───────────────────────────────────────────────
            // РЕЖИМ СОЗДАНИЯ КАРТОЧКИ
            // ───────────────────────────────────────────────
            if (_cardId == null)
            {
                _tempList.Add(comp);
                _refresh();
                NavigationService.GoBack();
                return;
            }

            // ───────────────────────────────────────────────
            // РЕЖИМ РЕДАКТИРОВАНИЯ КАРТОЧКИ
            // ───────────────────────────────────────────────
            comp.CardID = _cardId.Value;

            _context.AssetComponents.Add(comp);
            _context.SaveChanges();

            _refresh();
            NavigationService.GoBack();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
