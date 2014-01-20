/*
This file is part of Ext JS 3.4

Copyright (c) 2011-2013 Sencha Inc

Contact:  http://www.sencha.com/contact

Commercial Usage
Licensees holding valid commercial licenses may use this file in accordance with the Commercial
Software License Agreement provided with the Software or, alternatively, in accordance with the
terms contained in a written agreement between you and Sencha.

If you are unsure which license is appropriate for your use, please contact the sales department
at http://www.sencha.com/contact.

Build date: 2013-04-03 15:07:25
*/
/**
 * @class Ext.layout.MenuLayout
 * @extends Ext.layout.ContainerLayout
 * <p>Layout manager used by {@link Ext.menu.Menu}. Generally this class should not need to be used directly.</p>
 */
 Ext.layout.MenuLayout = Ext.extend(Ext.layout.ContainerLayout, {
    monitorResize : true,

    type: 'menu',

    setContainer : function(ct){
        this.monitorResize = !ct.floating;
        // This event is only fired by the menu in IE, used so we don't couple
        // the menu with the layout.
        ct.on('autosize', this.doAutoSize, this);
        Ext.layout.MenuLayout.superclass.setContainer.call(this, ct);
    },

    renderItem : function(c, position, target){
        if (!this.itemTpl) {
            this.itemTpl = Ext.layout.MenuLayout.prototype.itemTpl = new Ext.XTemplate(
                '<li id="{itemId}" class="{itemCls}">',
                    '<tpl if="needsIcon">',
                        '<img alt="{altText}" src="{icon}" class="{iconCls}"/>',
                    '</tpl>',
                '</li>'
            );
        }

        if(c && !c.rendered){
            if(Ext.isNumber(position)){
                position = target.dom.childNodes[position];
            }
            var a = this.getItemArgs(c);

//          The Component's positionEl is the <li> it is rendered into
            c.render(c.positionEl = position ?
                this.itemTpl.insertBefore(position, a, true) :
                this.itemTpl.append(target, a, true));

//          Link the containing <li> to the item.
            c.positionEl.menuItemId = c.getItemId();

//          If rendering a regular Component, and it needs an icon,
//          move the Component rightwards.
            if (!a.isMenuItem && a.needsIcon) {
                c.positionEl.addClass('x-menu-list-item-indent');
            }
            this.configureItem(c);
        }else if(c && !this.isValidParent(c, target)){
            if(Ext.isNumber(position)){
                position = target.dom.childNodes[position];
            }
            target.dom.insertBefore(c.getActionEl().dom, position || null);
        }
    },

    getItemArgs : function(c) {
        var isMenuItem = c instanceof Ext.menu.Item,
            canHaveIcon = !(isMenuItem || c instanceof Ext.menu.Separator);

        return {
            isMenuItem: isMenuItem,
            needsIcon: canHaveIcon && (c.icon || c.iconCls),
            icon: c.icon || Ext.BLANK_IMAGE_URL,
            iconCls: 'x-menu-item-icon ' + (c.iconCls || ''),
            itemId: 'x-menu-el-' + c.id,
            itemCls: 'x-menu-list-item ',
            altText: c.altText || ''
        };
    },

    //  Valid if the Component is in a <li> which is part of our target <ul>
    isValidParent : function(c, target) {
        return c.el.up('li.x-menu-list-item', 5).dom.parentNode === (target.dom || target);
    },

    onLayout : function(ct, target){
        Ext.layout.MenuLayout.superclass.onLayout.call(this, ct, target);
        this.doAutoSize();
    },

    doAutoSize : function(){
        var ct = this.container, w = ct.width;
        if(ct.floating){
            if(w){
                ct.setWidth(w);
            }else if(Ext.isIE9m){
                ct.setWidth(Ext.isStrict && (Ext.isIE7 || Ext.isIE8 || Ext.isIE9) ? 'auto' : ct.minWidth);
                var el = ct.getEl(), t = el.dom.offsetWidth; // force recalc
                ct.setWidth(ct.getLayoutTarget().getWidth() + el.getFrameWidth('lr'));
            }
        }
    }
});
Ext.Container.LAYOUTS['menu'] = Ext.layout.MenuLayout;
