﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using ShaderTools.CodeAnalysis.Editor.Commands;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.Completion
{
    internal partial class Controller
    {
        CommandState ICommandHandler<PageUpKeyCommandArgs>.GetCommandState(PageUpKeyCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return nextHandler();
        }

        CommandState ICommandHandler<PageDownKeyCommandArgs>.GetCommandState(PageDownKeyCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return nextHandler();
        }

        internal bool TryHandleUpKey()
        {
            AssertIsForeground();
            return ChangeSelection(() => sessionOpt.PresenterSession.SelectPreviousItem());
        }

        internal bool TryHandleDownKey()
        {
            AssertIsForeground();
            return ChangeSelection(() => sessionOpt.PresenterSession.SelectNextItem());
        }

        void ICommandHandler<PageUpKeyCommandArgs>.ExecuteCommand(PageUpKeyCommandArgs args, Action nextHandler)
        {
            AssertIsForeground();
            if (!ChangeSelection(() => sessionOpt.PresenterSession.SelectPreviousPageItem()))
            {
                nextHandler();
            }
        }

        void ICommandHandler<PageDownKeyCommandArgs>.ExecuteCommand(PageDownKeyCommandArgs args, Action nextHandler)
        {
            AssertIsForeground();
            if (!ChangeSelection(() => sessionOpt.PresenterSession.SelectNextPageItem()))
            {
                nextHandler();
            }
        }

        private bool ChangeSelection(Action computationAction)
        {
            AssertIsForeground();

            var result = ChangeSelectionWorker();
            if (result)
            {
                // We have a completion list and we want to process this navigation command ourselves.
                computationAction();
            }
            else
            {
                // We want the editor do process this navigation comment.
                DismissSessionIfActive();
            }

            return result;
        }

        private bool ChangeSelectionWorker()
        {
            AssertIsForeground();

            if (!IsSessionActive)
            {
                // No computation running, so just let the editor handle this.
                return false;
            }

            // We have a computation running, but it hasn't computed any results yet. As far as 
            // the user is concerned, they're just trying to navigate within the file.  We do
            // not want to block the user here.
            if (sessionOpt.Computation.InitialUnfilteredModel == null)
            {
                // Dismiss ourselves and actually allow the editor to navigate.
                return false;
            }

            // If we've finished computing the initial set of completions then wait for any 
            // current work to be finished and for the UI to be updated accordingly (this should
            // be fast, so waiting should not be an issue here).  Then actually perform the 
            // operation on the up to date completion list.
            var model = sessionOpt.Computation.WaitForController();

            return model != null;
        }
    }
}
