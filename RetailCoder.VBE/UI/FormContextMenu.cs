﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Office.Core;
using Microsoft.Vbe.Interop;
using Rubberduck.Parsing;
using Rubberduck.Refactorings.Rename;
using Rubberduck.UI.Refactorings;

namespace Rubberduck.UI
{
    internal class FormContextMenu
    {
        private readonly IRubberduckParser _parser;
        private readonly VBE _vbe;

        // ReSharper disable once NotAccessedField.Local
        private CommandBarButton _rename;

        public FormContextMenu(VBE vbe, IRubberduckParser parser)
        {
            _vbe = vbe;
            _parser = parser;
        }

        public void Initialize()
        {
            var beforeItem = _vbe.CommandBars["MSForms Control"].Controls.Cast<CommandBarControl>().First(control => control.Id == 2558).Index;
            _rename = _vbe.CommandBars["MSForms Control"].Controls.Add(Type: MsoControlType.msoControlButton, Temporary: true, Before: beforeItem) as CommandBarButton;
            _rename.BeginGroup = true;
            _rename.Caption = RubberduckUI.Rename;
            _rename.Click += OnRenameButtonClick;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private void OnRenameButtonClick(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            if (_vbe.ActiveCodePane == null)
            {
                return;
            }

            Rename();
        }

        private void Rename()
        {
            var progress = new ParsingProgressPresenter();
            var result = progress.Parse(_parser, _vbe.ActiveVBProject);

            using (var view = new RenameDialog())
            {
                var factory = new RenamePresenterFactory(_vbe, view, result);
                var refactoring = new RenameRefactoring(factory);
                refactoring.Refactor();
            }
        }

        /*public void Rename(Declaration target)
        {
            var progress = new ParsingProgressPresenter();
            var result = progress.Parse(_parser, IDE.ActiveVBProject);

            using (var view = new RenameDialog())
            {
                var factory = new RenamePresenterFactory(IDE, view, result);
                var refactoring = new RenameRefactoring(factory);
                refactoring.Refactor(target);
            }
        }*/

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) { return; }

            _rename.Click -= OnRenameButtonClick;
            _rename.Delete();
        }
    }
}
