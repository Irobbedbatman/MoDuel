using System;
using System.Linq;

namespace MoDuel {

    [MoonSharp.Interpreter.MoonSharpUserData]
    public class CanPlayResponse {

        /// <summary>
        /// The types of response a canplay action should return. 
        /// <para><see cref="CanPlayResponse"/> convert <see cref="Reason"/> to <see cref="Responses"/> implicitly and don't actually need to have one provided.</para>
        /// <para><see cref="Erroneous"/> should be only used when <see cref="Custom"/> makes no sense. Example: the card doesn't exist or wasn't in the hand.</para>
        /// </summary>
        [Flags]
        public enum Responses {
            CanPlay = 0,
            NotEnoughMana = 1,
            NoTarget = 2,
            WrongTarget = 4,
            Erroneous = 8,
            Custom = 16,
            Test = 32,
            Empty = 64
        };

        /// <summary>
        /// The reasons this card can't be played.
        /// </summary>
        public string[] Reasons = new string[0];

        public CanPlayResponse(params string[] reasons) {
            if (reasons == null)
                Reasons = new string[0];
            else
                Reasons = reasons;
        }

        /// <summary>
        /// The implicit converter that can be used to see if a card can be played.
        /// </summary>
        public static implicit operator bool(CanPlayResponse response) {
            if (response.Reasons.Length == 0)
                return true;
            foreach (string reason in response.Reasons) {
                //If any reason is invalid it invalidates the whole CanPlayResponse.
                bool valid = (reason == "") | (reason.ToLower() == "true") | (reason.ToLower() == "canplay");
                if (!valid)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// The implicit converter of <see cref="Reason"/> into one of the <see cref="Responses"/>.
        /// </summary>
        public static implicit operator Responses(CanPlayResponse response) {
            //Check to see if it can be played.
            if (response.CanPlay)
                return Responses.CanPlay;
            if (response.IsError)
                return Responses.Erroneous;
            //Check to see if any reasons can be parsed literraly.
            Responses responsesEnum = Responses.Empty;
            foreach (var reason in response.Reasons) {
                if (Enum.TryParse(reason, out Responses result))
                    responsesEnum |= result;
                else
                    responsesEnum |= Responses.Custom;
            }
            return responsesEnum;
        }

        /// <summary>
        /// Creates a new <see cref="CanPlayResponse"/>.
        /// <para>This exists so that <see cref="DuelFlow"/>Lua can shorthand response creation.</para>
        /// </summary>
        public static CanPlayResponse New() => new CanPlayResponse();

        /// <summary>
        /// Accessor of this as a bool for readability.
        /// </summary>
        public bool CanPlay => this;

        /// <summary>
        /// Checks to see if this falls under <see cref="Responses.Erroneous"/>
        /// <para>Used primarily to solve ambiquity in reasons.</para>
        /// </summary>
        public bool IsError {
            get {
                foreach (var reason in Reasons) {
                    //If any reason is errouneous the whole CanPlayResponse is erroneous.
                    bool error = (reason.ToLower() == "erroneous") | (reason.ToLower() == "err") | (reason.ToLower() == "error");
                    if (error)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Adds a new reason to this <see cref="CanPlayResponse"/>.
        /// </summary>
        public CanPlayResponse Extend(string reason) {
            var list = Reasons.ToList();
            list.Add(reason);
            Reasons = list.ToArray();
            return this;
        }

    }
}
