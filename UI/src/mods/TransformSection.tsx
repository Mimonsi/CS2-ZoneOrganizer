import { useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { ModuleRegistryExtend } from "cs2/modding";
import { Brush, Entity, tool } from "cs2/bindings"
import { Dropdown, DropdownItem, DropdownToggle } from "cs2/ui";
import { createElement } from "react";
import { entityEquals, entityKey } from "cs2/utils";
import { PropsSlider, SliderValueTransformer, Slider } from "../../game-ui/common/input/slider/slider";
import { PropsSection, Section } from "../../game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options";
import { PropsTextInput, TextInput, TextInputType } from "../../game-ui/common/input/text/text-input";
import { FOCUS_DISABLED$ } from "../../game-ui/common/focus/focus-key";


export const TransformSection: ModuleRegistryExtend = (Component: any) => {
	return (props) => {
		// translation handling. Translates using locale keys that are defined in C# or fallback string here.
		// const { translate } = useLocalization();

		// This defines aspects of the components.
		//const { children, ...otherProps } = props || {};

		console.log(props);

		// This gets the original component that we may alter and return.
		var result = Component();

		console.log(result);

		return props;
	};
}